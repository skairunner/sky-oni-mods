using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using SkyLib;

namespace RockCrusherConfiguration
{
    public class RockCrusherOption: SingletonOption<RockCrusherOption>
    {
        [RestartRequired]
        [Option("Lime fraction", "The % of fossil that is converted to Lime at a Rock Crusher.")]
        [Limit(0, 100)]
        [JsonProperty]
        public int LimeFraction { get; set; } = 5;

        [RestartRequired]
        [Option("Refinement fraction", "The % of ore that is converted to Refined Metal at a Rock Crusher.")]
        [Limit(0, 100)]
        [JsonProperty]
        public int RefineFraction { get; set; } = 50;

        [RestartRequired]
        [Option("Salt amount", "The amount of Table Salt in grams that is produced from Salt a Rock Crusher.")]
        [JsonProperty]
        public float SaltAmount { get; set; } = 5f;
    }
}