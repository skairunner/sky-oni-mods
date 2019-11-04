using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib;

namespace AdvancedStart
{
    public enum Config
    {
        DefaultStart,
        AdvancedStart,
        SpaceStart,
        Custom
    }
    public class AdvancedStartOptions
    {
        [Option("Start Profile", "The Advanced Start profile to use.")][JsonProperty] private Config config { get; set; }

        public AdvancedStartConfig GetProfile()
        {
            
        }
    }

    public class AdvancedStartConfig
    {
        public int startSkillPoints;
        public int startAttributeBoost;
        public List<string> startTechs;
        public Dictionary<string, float> startItems;
    }
}