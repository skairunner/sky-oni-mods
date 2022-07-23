using System.Reflection;

namespace SkyLib
{
    public class Logger
    {
        public static string GetModName(Assembly mod)
        {
            return ((AssemblyTitleAttribute) mod.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
        }

        // Start only once per instance.
        public static void StartLogging()
        {
            var assembly = Assembly.GetCallingAssembly();
            LogLine($"SkyLib");
        }

        public static void LogLine(string line)
        {
            var assembly = Assembly.GetCallingAssembly();
            Debug.LogFormat("[{0}] {1}", GetModName(assembly), line);
        }
    }
}
