using System;
using System.Threading;
using qmap_v1.UI;
using qmap_v1.Core;

namespace qmap_v1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            LoadingScreen.Show();
            Shell.Run(args);
        }
    }
}
