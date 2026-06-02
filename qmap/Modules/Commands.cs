using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using qmap_v1.Core;
using qmap_v1.UI;

namespace qmap_v1.Modules
{
    internal sealed class ScanCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0) { Renderer.Error("Usage: scan <subnet> [-a]  e.g. scan 192.168.1.0/24"); return; }

            string target  = args[0];
            bool showAll   = args.Length > 1 && string.Equals(args[1], "-a", StringComparison.OrdinalIgnoreCase);

            var hosts = SubnetParser.Expand(target);
            if (hosts == null) { Renderer.Error("Invalid subnet format. Use CIDR notation: 192.168.1.0/24"); return; }

            Renderer.Section($"Host Discovery  ─  {target}{(showAll ? "  [all]" : "")}");
            Renderer.TableHeader("IP Address", "Status", "Hostname", "RTT");

            int alive = 0;
            object consoleLock = new object();

            Parallel.ForEach(hosts, new ParallelOptions { MaxDegreeOfParallelism = 64 }, ip =>
            {
                using (var ping = new Ping())
                {
                    try
                    {
                        var reply = ping.Send(ip, 800);
                        bool up   = reply != null && reply.Status == IPStatus.Success;
                        if (up)
                        {
                            string host = ResolveHost(ip);
                            Interlocked.Increment(ref alive);
                            lock (consoleLock)
                                Renderer.TableRow(ip, "UP", host, reply.RoundtripTime + " ms");
                        }
                        else if (showAll)
                        {
                            lock (consoleLock)
                                Renderer.TableRow(ip, "DOWN", "—", "—");
                        }
                    }
                    catch { if (showAll) lock (consoleLock) Renderer.TableRow(ip, "DOWN", "—", "—"); }
                }
            });

            Renderer.SectionEnd();
            Renderer.Success($"{alive} host(s) up out of {hosts.Count} scanned.");
        }

        private static string ResolveHost(string ip)
        {
            try { return Dns.GetHostEntry(ip).HostName; }
            catch { return "—"; }
        }
    }

    internal sealed class PingCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0) { Renderer.Error("Usage: ping <host> [count]"); return; }

            string host  = args[0];
            int count    = args.Length > 1 && int.TryParse(args[1], out int n) ? n : 4;

            Renderer.Section($"Ping  ─  {host}");

            long total = 0; int success = 0;
            using (var ping = new Ping())
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var reply = ping.Send(host, 3000);
                        if (reply == null) { Renderer.Row($"seq={i + 1}", "no response"); continue; }
                        if (reply.Status == IPStatus.Success)
                        {
                            success++;
                            total += reply.RoundtripTime;
                            string ttl = (reply.Options != null) ? reply.Options.Ttl.ToString() : "—";
                            Renderer.Row($"seq={i + 1}", $"{reply.Address}  rtt={reply.RoundtripTime} ms  ttl={ttl}");
                        }
                        else
                        {
                            Renderer.Row($"seq={i + 1}", reply.Status.ToString());
                        }
                    }
                    catch (Exception ex) { Renderer.Row($"seq={i + 1}", "error: " + ex.Message); }
                    Thread.Sleep(200);
                }
            }

            Renderer.SectionEnd();
            Renderer.Success($"{success}/{count} replies  avg={( success > 0 ? total / success : 0)} ms");
        }
    }

    internal sealed class PortCommand : ICommand
    {
        private static readonly int[] _commonPorts = new[]
        {
            21,22,23,25,53,80,110,143,443,445,
            3306,3389,5432,6379,8080,8443,27017
        };

        public void Execute(string[] args)
        {
            if (args.Length == 0) { Renderer.Error("Usage: port <host> [start-end | port]"); return; }

            string host = args[0];
            int[]  ports;

            if (args.Length > 1)
            {
                var parts = args[1].Split('-');
                if (parts.Length == 2
                    && int.TryParse(parts[0], out int s) && int.TryParse(parts[1], out int e)
                    && s >= 1 && e <= 65535 && s <= e)
                {
                    var list = new List<int>();
                    for (int p = s; p <= e; p++) list.Add(p);
                    ports = list.ToArray();
                }
                else if (int.TryParse(args[1], out int single) && single >= 1 && single <= 65535)
                    ports = new[] { single };
                else
                    { Renderer.Error("Invalid port specification. Use a number (1-65535) or range e.g. 1-1024"); return; }
            }
            else
                ports = _commonPorts;

            Renderer.Section($"Port Scan  ─  {host}");
            Renderer.TableHeader("Port", "State", "Service");

            int open = 0;
            object lk = new object();

            Parallel.ForEach(ports, new ParallelOptions { MaxDegreeOfParallelism = 100 }, port =>
            {
                bool isOpen = IsOpen(host, port);
                if (isOpen)
                {
                    Interlocked.Increment(ref open);
                    lock (lk)
                        Renderer.TableRow(port.ToString(), "OPEN", ServiceName(port));
                }
            });

            Renderer.SectionEnd();
            Renderer.Success($"{open} open port(s) found.");
        }

        private static bool IsOpen(string host, int port)
        {
            try
            {
                using (var tcp = new TcpClient())
                {
                    var result = tcp.BeginConnect(host, port, null, null);
                    bool connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(600));
                    if (connected)
                    {
                        try { tcp.EndConnect(result); }
                        catch { return false; }
                        return tcp.Connected;
                    }
                    return false;
                }
            }
            catch { return false; }
        }

        private static string ServiceName(int port)
        {
            switch (port)
            {
                case 21:    return "FTP";
                case 22:    return "SSH";
                case 23:    return "Telnet";
                case 25:    return "SMTP";
                case 53:    return "DNS";
                case 80:    return "HTTP";
                case 110:   return "POP3";
                case 143:   return "IMAP";
                case 443:   return "HTTPS";
                case 445:   return "SMB";
                case 3306:  return "MySQL";
                case 3389:  return "RDP";
                case 5432:  return "PostgreSQL";
                case 6379:  return "Redis";
                case 8080:  return "HTTP-Alt";
                case 8443:  return "HTTPS-Alt";
                case 27017: return "MongoDB";
                default:    return "unknown";
            }
        }
    }

    internal sealed class DnsCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0) { Renderer.Error("Usage: dns <host>"); return; }

            string host = args[0];
            Renderer.Section($"DNS Lookup  ─  {host}");

            try
            {
                var entry = Dns.GetHostEntry(host);
                Renderer.Row("Hostname", entry.HostName);
                foreach (var addr in entry.AddressList)
                    Renderer.Row(addr.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4", addr.ToString());
                foreach (var alias in entry.Aliases)
                    Renderer.Row("Alias", alias);
            }
            catch (Exception ex) { Renderer.Error(ex.Message); }

            Renderer.SectionEnd();
        }
    }

    internal sealed class TraceCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0) { Renderer.Error("Usage: trace <host>"); return; }

            string host    = args[0];
            int maxHops    = 30;
            int timeout    = 3000;

            Renderer.Section($"Traceroute  ─  {host}");
            Renderer.TableHeader("Hop", "Address", "RTT");

            using (var ping = new Ping())
            {
                for (int ttl = 1; ttl <= maxHops; ttl++)
                {
                    var options = new PingOptions(ttl, true);
                    try
                    {
                        var reply = ping.Send(host, timeout, new byte[32], options);
                        string addr = (reply.Address != null) ? reply.Address.ToString() : "*";
                        string rtt  = reply.RoundtripTime > 0 ? reply.RoundtripTime + " ms" : "*";
                        Renderer.TableRow(ttl.ToString(), addr, rtt);

                        if (reply.Status == IPStatus.Success) break;
                    }
                    catch (PingException) { Renderer.TableRow(ttl.ToString(), "*", "request timed out"); }
                    catch { Renderer.TableRow(ttl.ToString(), "*", "*"); }
                }
            }

            Renderer.SectionEnd();
        }
    }

    internal sealed class WhoisCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0) { Renderer.Error("Usage: whois <domain>"); return; }

            string domain = args[0];
            Renderer.Section($"Whois  ─  {domain}");

            try
            {
                string raw = QueryWhois(domain, "whois.iana.org");
                Renderer.Info(raw.Length > 0 ? "Raw whois data retrieved:" : "No data returned.");
                if (raw.Length > 0)
                {
                    foreach (var line in raw.Split('\n'))
                    {
                        string trimmed = line.Trim();
                        if (trimmed.Length > 0 && !trimmed.StartsWith("%") && !trimmed.StartsWith("#"))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine("  │  " + trimmed);
                        }
                    }
                    Console.ResetColor();
                }
            }
            catch (Exception ex) { Renderer.Error(ex.Message); }

            Renderer.SectionEnd();
        }

        private static string QueryWhois(string domain, string server)
        {
            using (var tcp = new TcpClient(server, 43))
            using (var stream = tcp.GetStream())
            {
                stream.ReadTimeout  = 5000;
                stream.WriteTimeout = 3000;
                byte[] data = System.Text.Encoding.ASCII.GetBytes(domain + "\r\n");
                stream.Write(data, 0, data.Length);
                var buffer = new byte[8192];
                var sb     = new System.Text.StringBuilder();
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    sb.Append(System.Text.Encoding.ASCII.GetString(buffer, 0, read));
                return sb.ToString();
            }
        }
    }

    internal sealed class NetInfoCommand : ICommand
    {
        public void Execute(string[] args)
        {
            Renderer.Section("Network Interfaces");

            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus != OperationalStatus.Up) continue;

                Renderer.Row("Interface", iface.Name);
                Renderer.Row("Type",      iface.NetworkInterfaceType.ToString());
                string mac = iface.GetPhysicalAddress().ToString();
                Renderer.Row("MAC", mac.Length > 0 ? mac : "—");
                string speed = iface.Speed > 0 ? (iface.Speed / 1_000_000) + " Mbps" : "unknown";
                Renderer.Row("Speed", speed);

                var props = iface.GetIPProperties();
                foreach (var ua in props.UnicastAddresses)
                {
                    string family = ua.Address.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4";
                    Renderer.Row(family, ua.Address + " / " + ua.PrefixLength);
                }

                foreach (var gw in props.GatewayAddresses)
                    Renderer.Row("Gateway", gw.Address.ToString());

                foreach (var dns in props.DnsAddresses)
                    Renderer.Row("DNS", dns.ToString());

                Renderer.Blank();
            }

            Renderer.SectionEnd();
        }
    }

    internal sealed class ClearCommand : ICommand
    {
        public void Execute(string[] args)
        {
            Console.Clear();
            Renderer.Header();
            Renderer.Blank();
        }
    }

    internal sealed class VersionCommand : ICommand
    {
        public void Execute(string[] args)
        {
            Renderer.Section("Version Info");
            Renderer.Row("qmap",      Meta.Version);
            Renderer.Row("Runtime",   ".NET Framework 4.8");
            Renderer.Row("Language",  "C# 7.3");
            Renderer.Row("Platform",  "Windows");
            Renderer.SectionEnd();
        }
    }

    internal sealed class HelpCommand : ICommand
    {
        public void Execute(string[] args)
        {
            Renderer.Section("Available Commands");
            Renderer.TableHeader("Command", "Usage", "Description");
            Renderer.TableRow("scan",    "scan <subnet> [-a]",    "Discover live hosts (-a shows all)");
            Renderer.TableRow("ping",    "ping <host> [count]",   "ICMP ping a host");
            Renderer.TableRow("port",    "port <host> [range]",   "Scan ports on a host");
            Renderer.TableRow("dns",     "dns <host>",            "Resolve DNS records");
            Renderer.TableRow("trace",   "trace <host>",          "Traceroute to a host");
            Renderer.TableRow("whois",   "whois <domain>",        "WHOIS lookup");
            Renderer.TableRow("net",     "net",                   "List local network interfaces");
            Renderer.TableRow("version", "version",               "Show version info");
            Renderer.TableRow("clear",   "clear",                 "Clear the screen");
            Renderer.TableRow("help",    "help",                  "Show this help");
            Renderer.TableRow("exit",    "exit",                  "Quit qmap");
            Renderer.SectionEnd();
        }
    }
}
