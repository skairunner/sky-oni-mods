using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using SkyLib;

namespace CarbonRevolution
{
    public class CarbonOption: SingletonOption<CarbonOption>
    {
        
        [RestartRequired]
        [Option("Coal Gen CO2", "How much CO2 in kg/s the Coal Gen emits.")][JsonProperty]
        public float CO2_coalgen { get; set; } = .25f;

        [RestartRequired]
        [Option("Petrol Gen CO2", "How much CO2 in kg/s the Petrol Gen emits.")][JsonProperty]
        public float CO2_petrolgen { get; set; } = .5f;
        
        [RestartRequired]
        [Option("Lumber Gen CO2", "How much CO2 in kg/s the Lumber Gen emits.")][JsonProperty]
        public float CO2_lumbergen { get; set; } = .17f;

        [RestartRequired]
        [Limit(0, 1)]
        [Option("Nat Gas CO2", "How much CO2 in kg/s the Nat Gas Gen emits.")]
        [JsonProperty]
        public float CO2_natgasgen { get; set; } = .14f;

        [RestartRequired]
        [Option("Gas Range CO2", "How much CO2 in kg/s the Gas Range emits.")][JsonProperty]
        public float CO2_gasrange { get; set; } = .1f;
        
        [RestartRequired]
        [Option("Ethanol Distiller CO2", "How much CO2 in kg/s the Ethanol Distiller emits.")][JsonProperty] 
        public float CO2_ethanoldistiller { get; set; } = .16667f;
    }
}