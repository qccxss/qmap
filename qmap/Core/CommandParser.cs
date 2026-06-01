using System.Collections.Generic;
using System.Text;

namespace qmap_v1.Core
{
    internal static class CommandParser
    {
        public static string[] Parse(string input)
        {
            var tokens = new List<string>();
            var current = new StringBuilder();
            bool inQuote = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '"') { inQuote = !inQuote; continue; }
                if (c == ' ' && !inQuote)
                {
                    if (current.Length > 0) { tokens.Add(current.ToString()); current.Clear(); }
                    continue;
                }
                current.Append(c);
            }
            if (current.Length > 0) tokens.Add(current.ToString());
            return tokens.Count > 0 ? tokens.ToArray() : new[] { "" };
        }
    }
}
