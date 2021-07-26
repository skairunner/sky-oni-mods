using System;
using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;

namespace Drains
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("https://github.com/skairunner/sky-oni-mods")]
    [RestartRequired]
    public class DrainOptions: SingletonOptions<DrainOptions>
    {
        [JsonProperty]
        [Option("Solid Drains", "Drains will be solid and absorb water on the cell above them.")]
        public bool UseSolidDrain { get; set; }

        [JsonProperty]
        [Option("Flow Rate", "Determines the rate of liquid intake measured in kg/s.", Format = "F1")]
        [Limit(0.1f,1f)]
        public float FlowRate { get; set; }

        public DrainOptions()
        {
            UseSolidDrain = false;
            FlowRate = 0.1f;
        }
    }
}
