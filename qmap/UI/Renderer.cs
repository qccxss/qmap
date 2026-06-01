using System;

namespace qmap_v1.UI
{
    internal static class Renderer
    {
        private static readonly string _divider = new string('─', 60);

        public static void Header()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ★ qmap " + Meta.Version + "  |  " + Meta.Tagline);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + _divider);
            Console.ResetColor();
        }

        public static void Prompt()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  qmap » ");
            Console.ResetColor();
        }

        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  [*] " + message);
            Console.ResetColor();
        }

        public static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  [+] " + message);
            Console.ResetColor();
        }

        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [!] " + message);
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  [-] " + message);
            Console.ResetColor();
        }

        public static void Section(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─ " + title.ToUpper());
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  │");
            Console.ResetColor();
        }

        public static void Row(string key, string value)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(key.PadRight(22));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void SectionEnd()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  └" + new string('─', 40));
            Console.ResetColor();
        }

        public static void Divider()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + _divider);
            Console.ResetColor();
        }

        public static void Blank() => Console.WriteLine();

        public static void TableHeader(params string[] cols)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  ├ ");
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var col in cols)
                Console.Write(col.PadRight(20));
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ├" + new string('─', 58));
            Console.ResetColor();
        }

        public static void TableRow(params string[] cells)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │ ");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var cell in cells)
                Console.Write(cell.PadRight(20));
            Console.WriteLine();
            Console.ResetColor();
        }
    }

    internal static class Meta
    {
        public const string Version = "v0.0.1";
        public const string Tagline = "Advanced Network Mapping";
    }
}
