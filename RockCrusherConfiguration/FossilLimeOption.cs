using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using SkyLib;

namespace RockCrusherConfiguration
{
    public class FossilLimeOption: SingletonOption<FossilLimeOption>
    {
        [RestartRequired]
        [Option("Lime fraction", "The % of fossil that is converted to Lime at a Rock Crusher.")]
        [JsonProperty]
        public int LimeFraction { get; set; } = 5;
    }
}