using System.Reflection;

namespace SkyLib
{
    public class Logger
    {
        public static string GetModName(Assembly mod)
        {
            return ((AssemblyTitleAttribute) mod.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
        }

        public static string GetModVersion(Assembly mod)
        {
            return ((AssemblyInformationalVersionAttribute)
                mod.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0]).InformationalVersion;
        }

        // Start only once per instance.
        public static void StartLogging()
        {
            var assembly = Assembly.GetCallingAssembly();
            LogLine($"Started with version {GetModVersion(assembly)}.");
        }

        public static void LogLine(string line)
        {
            var assembly = Assembly.GetCallingAssembly();
            Debug.LogFormat("[{0}] {1}", GetModName(assembly), line);
        }
    }
}
