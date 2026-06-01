using System;
using System.Threading;

namespace qmap_v1.UI
{
    internal static class LoadingScreen
    {
        private static readonly string[] _logo = new[]
        {
            @"",
            @"        ██████╗ ███╗   ███╗ █████╗ ██████╗ ",
            @"       ██╔═══██╗████╗ ████║██╔══██╗██╔══██╗",
            @"       ██║   ██║██╔████╔██║███████║██████╔╝",
            @"       ██║▄▄ ██║██║╚██╔╝██║██╔══██║██╔═══╝ ",
            @"       ╚██████╔╝██║ ╚═╝ ██║██║  ██║██║     ",
            @"        ╚══▀▀═╝ ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝     ",
            @"",
        };

        private static readonly string[] _stars = new[]
        {
            "  ✦       ✧          ✦    ✧       ✦   ",
            "     ✧       ✦    ✧      ✦    ✧      ",
            "  ✦    ✧       ✦      ✧      ✦    ✧  ",
        };

        private static readonly string[] _spinnerFrames = new[]
        {
            "◢", "◣", "◤", "◥"
        };

        private static readonly string[] _barChars = new[]
        {
            "░", "▒", "▓", "█"
        };

        public static void Show()
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            int width = Math.Max(Console.WindowWidth, 60);
            int topPad = 2;

            DrawStarField(topPad, width);
            DrawLogo(topPad + 3);
            DrawTagline(topPad + 12, width);
            DrawLoadingBar(topPad + 15, width);

            Thread.Sleep(300);
            Console.Clear();
            Console.CursorVisible = true;
            Console.ResetColor();
        }

        private static void DrawStarField(int top, int width)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            for (int i = 0; i < _stars.Length; i++)
            {
                SetCursor(top + i, 0);
                string line = _stars[i % _stars.Length];
                Console.Write(line.PadRight(width).Substring(0, Math.Min(line.Length + 10, width)));
            }
        }

        private static void DrawLogo(int top)
        {
            for (int i = 0; i < _logo.Length; i++)
            {
                SetCursor(top + i, 0);

                if (i == 0 || i == _logo.Length - 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                else
                {
                    int brightness = Math.Abs(i - _logo.Length / 2);
                    Console.ForegroundColor = brightness <= 1
                        ? ConsoleColor.White
                        : brightness <= 2
                            ? ConsoleColor.Gray
                            : ConsoleColor.DarkGray;
                }

                Console.Write(_logo[i]);
                Thread.Sleep(60);
            }
        }

        private static void DrawTagline(int top, int width)
        {
            string version  = "v0.0.2";
            string tagline  = "Advanced Open Source Network Mapping Tool";
            string divider  = new string('─', Math.Min(tagline.Length + 4, width - 4));

            Console.ForegroundColor = ConsoleColor.DarkGray;
            SetCursorCenter(top, divider, width);

            Console.ForegroundColor = ConsoleColor.Gray;
            SetCursorCenter(top + 1, tagline, width);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            SetCursorCenter(top + 2, version, width);
            SetCursorCenter(top + 3, divider, width);
        }

        private static void DrawLoadingBar(int top, int width)
        {
            int barWidth   = Math.Min(40, width - 20);
            int barLeft    = (width - barWidth) / 2;
            int spinnerCol = barLeft + barWidth + 2;

            string[] stages = new[]
            {
                "Initializing engine",
                "Loading modules",
                "Loading commands",
                "Binding interfaces",
                "Welcome to qmap",
            };

            int totalTicks = barWidth;
            int ticksPerStage = totalTicks / stages.Length;

            for (int tick = 0; tick <= totalTicks; tick++)
            {
                int stageIndex = Math.Min(tick / ticksPerStage, stages.Length - 1);
                float pct      = (float)tick / totalTicks;

                SetCursor(top, barLeft - 1);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("[");

                for (int b = 0; b < barWidth; b++)
                {
                    float pos = (float)b / barWidth;
                    if (pos < pct - 0.15f)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("█");
                    }
                    else if (pos < pct)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("▓");
                    }
                    else if (pos < pct + 0.02f)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("▒");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("░");
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("]");

                Console.ForegroundColor = ConsoleColor.Gray;
                string pctStr = $" {(int)(pct * 100),3}%";
                Console.Write(pctStr);

                SetCursor(top + 2, barLeft - 1);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                string stage = stages[stageIndex].PadRight(30);
                Console.Write("» " + stage);

                SetCursor(top, spinnerCol);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(_spinnerFrames[tick % _spinnerFrames.Length]);

                Thread.Sleep(28);
            }

            SetCursor(top, spinnerCol);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("★");

            Thread.Sleep(400);
        }

        private static void SetCursor(int top, int left)
        {
            try { Console.SetCursorPosition(left, top); }
            catch { }
        }

        private static void SetCursorCenter(int top, string text, int width)
        {
            int left = Math.Max(0, (width - text.Length) / 2);
            SetCursor(top, left);
            Console.Write(text);
        }
    }
}
