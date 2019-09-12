using System;
using Harmony;

namespace SkyLib
{
    public class Logger
    {
        private static bool started = false;

        // Start only once per instance.
        public static void StartLogging(string modname = "")
        {
            if (!started)
            {
                LogLine("I'ms't'd've SkyLib!");
                started = true;
            }
            if (modname != "")
            {
                LogLine($"[{modname}] Started.");
            }
        }

        public static void LogLine(string modname, string line)
        {
            LogLine($"[{modname}] {line}");
        }

        public static void LogLine(string line)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"[{timestamp}] {line}");
        }
    }
}
