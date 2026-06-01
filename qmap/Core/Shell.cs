using System;
using System.Collections.Generic;
using qmap_v1.Modules;
using qmap_v1.UI;

namespace qmap_v1.Core
{
    internal static class Shell
    {
        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
        {
            { "scan",    new ScanCommand()    },
            { "ping",    new PingCommand()    },
            { "dns",     new DnsCommand()     },
            { "port",    new PortCommand()    },
            { "trace",   new TraceCommand()   },
            { "whois",   new WhoisCommand()   },
            { "net",     new NetInfoCommand() },
            { "help",    new HelpCommand()    },
        };

        public static void Run(string[] args)
        {
            Renderer.Header();
            Renderer.Blank();
            Renderer.Info("Type 'help' to list available commands. Type 'exit' to quit.");
            Renderer.Blank();

            if (args != null && args.Length > 0)
            {
                string joined = string.Join(" ", args);
                string[] parts = CommandParser.Parse(joined);
                Dispatch(parts);
                return;
            }

            while (true)
            {
                Renderer.Prompt();
                string input = Console.ReadLine();
                if (input == null) break;
                input = input.Trim();
                if (input.Length == 0) continue;
                if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
                    break;

                string[] parts = CommandParser.Parse(input);
                Dispatch(parts);
                Renderer.Blank();
            }

            Renderer.Info("Session terminated.");
        }

        private static void Dispatch(string[] parts)
        {
            string cmd = parts[0];
            string[] cmdArgs = parts.Length > 1
                ? ArraySlice(parts, 1)
                : new string[0];

            if (_commands.TryGetValue(cmd, out ICommand handler))
            {
                try { handler.Execute(cmdArgs); }
                catch (Exception ex) { Renderer.Error(ex.Message); }
            }
            else
            {
                Renderer.Error($"Unknown command: '{cmd}'.  Type 'help' for usage.");
            }
        }
        private static string[] ArraySlice(string[] arr, int start)
        {
            var result = new string[arr.Length - start];
            Array.Copy(arr, start, result, 0, result.Length);
            return result;
        }
    }
}
