using System;
using System.Reflection;
using Harmony;

namespace SkyLib
{
    public class Logger
    {

        public static string GetModName(Assembly mod)
        {
            return ((AssemblyModName)(mod.GetCustomAttributes(typeof(AssemblyModName), false)[0])).Value;
        }

        public static string GetModVersion(Assembly mod)
        {
            return ((AssemblyFileVersionAttribute)(mod.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0])).Version;
        }

        // Start only once per instance.
        public static void StartLogging()
        {
            var assembly = Assembly.GetCallingAssembly();
            LogLine($"Started with version {GetModVersion((assembly))}.");
        }

        public static void LogLine(string line)
        {
            var assembly = Assembly.GetCallingAssembly();
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"[{timestamp}][{GetModName(assembly)}] {line}");
        }
    }
}
