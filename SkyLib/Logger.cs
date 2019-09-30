using System;
using System.Reflection;
using Harmony;

namespace SkyLib
{
    public class Logger
    {

        public static string GetModName(Assembly mod)
        {
            return ((AssemblyProductAttribute)(mod.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0])).Product;
        }

        public static string GetModVersion(Assembly mod)
        {
            return ((AssemblyInformationalVersionAttribute)(mod.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0])).InformationalVersion;
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
