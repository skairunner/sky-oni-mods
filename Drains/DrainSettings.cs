using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;

namespace Drains
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DrainSettings
    {
        public DrainSettings()
        {
            flowRate = 0.1f;
        }

        [Option("Flow rate",
            "The flow rate of Drains. Between 0 and 1 kg/s.")]
        [Limit(0, 1)]
        [JsonProperty]
        public float flowRate { get; set; }

        public static DrainSettings GetSettings()
        {
            return POptions.ReadSettings<DrainSettings>();
        }
    }
}