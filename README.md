```
        ██████╗ ███╗   ███╗ █████╗ ██████╗ 
       ██╔═══██╗████╗ ████║██╔══██╗██╔══██╗
       ██║   ██║██╔████╔██║███████║██████╔╝
       ██║▄▄ ██║██║╚██╔╝██║██╔══██║██╔═══╝ 
       ╚██████╔╝██║ ╚═╝ ██║██║  ██║██║     
        ╚══▀▀═╝ ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝     
```

**Advanced Open-Source Network Mapping Tool**

![Platform](https://img.shields.io/badge/platform-Windows-lightgrey?style=flat-square)
![Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?style=flat-square)
![Language](https://img.shields.io/badge/language-C%23-239120?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-white?style=flat-square)
![Version](https://img.shields.io/badge/version-0.0.1-black?style=flat-square)

</div>

---

## Overview

**qmap** is a lightweight, command-line network mapping and reconnaissance tool built for Windows with .NET Framework 4.8. It provides a clean interactive shell with an animated loading screen and a structured black-and-white terminal UI for network diagnostics, host discovery, port scanning, and more.

---

## Features

- ★ Animated loading screen with ASCII art logo and progress bar
- ★ Interactive REPL shell with inline command support
- ★ Parallel host discovery via ICMP ping sweep
- ★ TCP port scanning with common service identification
- ★ DNS resolution, traceroute, and WHOIS lookup
- ★ Local network interface enumeration
- ★ Structured, readable black-and-white console output
- ★ No external dependencies — pure .NET Framework 4.8

---

## Requirements

| Requirement       | Detail                              |
|-------------------|-------------------------------------|
| OS                | Windows 7 / 10 / 11                 |
| Runtime           | .NET Framework 4.8                  |
| Build Tools       | Visual Studio 2019/2022 or MSBuild  |
| Privileges        | Administrator (recommended for ICMP)|

---

## Build

**Visual Studio:**

Open `qmap.csproj` and build with `Ctrl+Shift+B`.

**MSBuild (CLI):**

```
msbuild qmap.csproj /p:Configuration=Release
```

Output binary: `bin\Release\qmap.exe`

---

## Usage

### Interactive Shell

```
qmap.exe
```

Launches the REPL. Type commands interactively.

### Inline Mode

```
qmap.exe <command> [args]
```

Executes a single command and exits.

---

## Commands

| Command | Syntax                      | Description                              |
|---------|-----------------------------|------------------------------------------|
| `scan`  | `scan <subnet>`             | Discover live hosts on a CIDR subnet     |
| `ping`  | `ping <host> [count]`       | ICMP ping with optional repeat count     |
| `port`  | `port <host> [range\|port]` | TCP port scan — range or single port     |
| `dns`   | `dns <host>`                | Resolve IPv4, IPv6, and aliases          |
| `trace` | `trace <host>`              | Traceroute up to 30 hops                 |
| `whois` | `whois <domain>`            | WHOIS query via `whois.iana.org`         |
| `net`   | `net`                       | List local network interfaces            |
| `help`  | `help`                      | Display all commands                     |
| `exit`  | `exit`                      | Quit qmap                                |

### Examples

```
qmap.exe scan 192.168.1.0/24
qmap.exe ping 8.8.8.8 10
qmap.exe port 192.168.1.1 1-1024
qmap.exe port 192.168.1.1 443
qmap.exe dns example.com
qmap.exe trace example.com
qmap.exe whois example.com
qmap.exe net
```

---

## Project Structure

```
qmap/
├── Program.cs               Entry point — launches loading screen and shell
├── qmap.csproj              .NET Framework 4.8 project file
│
├── Core/
│   ├── ICommand.cs          Command interface
│   ├── CommandParser.cs     Quote-aware input tokenizer
│   └── Shell.cs             REPL loop and command dispatcher
│
├── Modules/
│   ├── Commands.cs          scan, ping, port, dns, trace, whois, net, help
│   └── SubnetParser.cs      CIDR notation to IP list expander
│
└── UI/
    ├── LoadingScreen.cs     ASCII logo, star field, animated progress bar
    └── Renderer.cs          Structured console output helpers
```

---

## Technical Notes

- `scan` and `port` use `Parallel.ForEach` with a configurable `MaxDegreeOfParallelism` for speed.
- `whois` connects directly to `whois.iana.org` on TCP port 43 — no third-party libraries.
- `trace` uses `PingOptions.Ttl` incremented per hop to simulate traceroute.
- ICMP operations may require Administrator privileges depending on Windows configuration.
- Built against C# 7.3 for full .NET Framework 4.8 compatibility — no language features required.
