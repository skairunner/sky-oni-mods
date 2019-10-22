using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PeterHan.PLib;
using PeterHan.PLib.Options;

namespace CarbonRevolution
{
    public class CarbonOption
    {
        public static CarbonOption Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = POptions.ReadSettings<CarbonOption>() ?? new CarbonOption();
                }

                return _Instance;
            }
            set => _Instance = value;
        }

        private static CarbonOption _Instance;

        private static float Clamp(float a, float x, float b)
        {
            return Math.Max(a, Math.Min(x, b));
        }

        public CarbonOption()
        {
            _CO2_natgasgen = .14f;
        }
        
        [Option("Coal Gen CO2", "How much CO2 in kg/s the Coal Gen emits.")][JsonProperty]
        public float CO2_coalgen { get; set; } = .25f;
        [Option("Petrol Gen CO2", "How much CO2 in kg/s the Petrol Gen emits.")][JsonProperty]
        public float CO2_petrolgen { get; set; } = .5f;
        [Option("Lumber Gen CO2", "How much CO2 in kg/s the Lumber Gen emits.")][JsonProperty]
        public float CO2_lumbergen { get; set; } = .17f;

        [Option("Nat Gas CO2", "How much CO2 in kg/s the Nat Gas Gen emits.")][JsonProperty]
        public float CO2_natgasgen
        {
            get => _CO2_natgasgen;
            set => _CO2_natgasgen = Clamp(0, value, 1);
        }

        [Option("Gas Range CO2", "How much CO2 in kg/s the Gas Range emits.")][JsonProperty]
        public float CO2_gasrange { get; set; } = .1f;
        [Option("Ethanol Distiller CO2", "How much CO2 in kg/s the Ethanol Distiller emits.")][JsonProperty] 
        public float CO2_ethanoldistiller { get; set; } = .16667f;

        private float _CO2_natgasgen;
    }
}