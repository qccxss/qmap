using System;
using System.Collections.Generic;
using System.Net;

namespace qmap_v1.Modules
{
    internal static class SubnetParser
    {
        public static List<string> Expand(string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2) return null;
            if (!IPAddress.TryParse(parts[0], out var baseAddr)) return null;
            if (!int.TryParse(parts[1], out int prefix) || prefix < 0 || prefix > 32) return null;

            uint ip   = ToUint(baseAddr);
            uint mask = prefix == 0 ? 0 : (0xFFFFFFFFu << (32 - prefix));
            uint net  = ip & mask;
            uint brd  = net | ~mask;

            var list = new List<string>();
            for (uint addr = net + 1; addr < brd; addr++)
                list.Add(FromUint(addr));
            return list;
        }

        private static uint ToUint(IPAddress addr)
        {
            byte[] b = addr.GetAddressBytes();
            return (uint)b[0] << 24 | (uint)b[1] << 16 | (uint)b[2] << 8 | b[3];
        }

        private static string FromUint(uint addr)
        {
            return $"{(addr >> 24) & 0xFF}.{(addr >> 16) & 0xFF}.{(addr >> 8) & 0xFF}.{addr & 0xFF}";
        }
    }
}
