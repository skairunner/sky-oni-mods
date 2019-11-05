using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;

namespace AdvancedStart
{
    public enum Config
    {
        [Option("Default start", "The default game start.")]
        DefaultStart,
        [Option("Advanced start", "A mid-game start.")]
        AdvancedStart,
        [Option("Space start", "An early space start.")]
        SpaceStart,
        [Option("Custom profile", "A custom start as defined in CustomStart.json.")]
        Custom
    }
    public class AdvancedStartOptions
    {
        [Option("Start Profile", "The Advanced Start profile to use.")]
        [JsonProperty]
        public Config config { get; set; } = Config.AdvancedStart;

        public static AdvancedStartConfig GetConfig()
        {
            var options = POptions.ReadSettings<AdvancedStartOptions>();
            if (options == null)
            {
                return new AdvancedStartOptions().GetProfile();
            }

            return options.GetProfile();
        }
        
        public AdvancedStartConfig GetProfile()
        {
            var baseDir = POptions.GetModDir(Assembly.GetExecutingAssembly());
            switch (config)
            {
                case Config.DefaultStart:
                    return new AdvancedStartConfig
                    {
                        startSkillPoints = 0,
                        startAttributeBoost = 0,
                        startTechs = new List<string>(),
                        startItems = new Dictionary<string, float>()
                    };
                case Config.AdvancedStart:
                    return JsonConvert.DeserializeObject<AdvancedStartConfig>(File.ReadAllText(Path.Combine(baseDir, "config/AdvancedStart.json")));
                case Config.SpaceStart:
                    return JsonConvert.DeserializeObject<AdvancedStartConfig>(File.ReadAllText(Path.Combine(baseDir, "config/SpaceStart.json")));
                default:
                    return JsonConvert.DeserializeObject<AdvancedStartConfig>(File.ReadAllText(Path.Combine(baseDir, "config/CustomStart.json")));
            }
        }
    }

    public struct AdvancedStartConfig
    {
        public int startSkillPoints;
        public int startAttributeBoost;
        public List<string> startTechs;
        public Dictionary<string, float> startItems;
    }
}