using System;
using System.Collections.Generic;
using System.Linq;
using MapStudio.UI;
using Newtonsoft.Json;

namespace TurboLibrary
{
    public enum UVersion
    {
        None = -1,
        Base = 0,
        Zelda,
        Animal,
        Mercedes,
        Modded,
    }

    public enum DXVersion
    {
        None = -1,
        Base = 0,
        BCP_1,
        BCP_2,
        BCP_3,
        BCP_4,
        BCP_5,
        BCP_6,
        Modded,
    }

    public class MetaPlatforms
    {
        private UVersion? _uVersion = null;
        private DXVersion? _dxVersion = null;
        public UVersion VersionU { get => _uVersion == null ? UVersion.Base : (UVersion)_uVersion; set => _uVersion = value; } // Parameter is used in the Wii U version; null assumes true
        public DXVersion VersionDX { get => _dxVersion == null ? DXVersion.Base : (DXVersion)_dxVersion; set => _dxVersion = value; } // Parameter is used in Deluxe on the Switch; null assumes true

        /// <summary>
        /// Merges this platform info with another one.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(MetaPlatforms other)
        {
            _uVersion = other._uVersion ?? _uVersion;
            _dxVersion = other._dxVersion ?? _dxVersion;
        }
    }

    /// <summary>
    /// Meta information for an object (<see cref="Obj"/>) placed in the course. This information is not present
    /// in the <c>objflow.byaml</c>, but serves as more user-friendly guidelines for editing.
    /// </summary>
    public class MapObjMeta
    {
        public bool IsDocumented { get; private set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";
        
        public List<string> Aliases { get; set; } = [];

        public List<string> Usages { get; set; } = []; // "U Sunshine Airport, ..."

        public ParamDescriptor[] Params { get; } = [
            new ParamDescriptor(0), new ParamDescriptor(1), new ParamDescriptor(2), new ParamDescriptor(3),
            new ParamDescriptor(4), new ParamDescriptor(5), new ParamDescriptor(6), new ParamDescriptor(7)
        ];

        // Wii U/Switch versions
        public MetaPlatforms Platforms { get; } = new();
        [JsonProperty("VersionU")] private UVersion VersionU { set => Platforms.VersionU = value; }
        [JsonProperty("VersionDX")] private DXVersion VersionDX { set => Platforms.VersionDX = value; }


        // Utilised by JSON deserializer
        private void setParam(int i, ParamDescriptor pd)
        {
            Params[i] = pd;
            Params[i].IsUsed = true;
        }

        // For deserialization. The JSON format is more user friendly when allowing parameters to be defined this way.
        [JsonProperty("param_0")] private ParamDescriptor Param1 { set => setParam(0, value); }
        [JsonProperty("param_1")] private ParamDescriptor Param2 { set => setParam(1, value); }
        [JsonProperty("param_2")] private ParamDescriptor Param3 { set => setParam(2, value); }
        [JsonProperty("param_3")] private ParamDescriptor Param4 { set => setParam(3, value); }
        [JsonProperty("param_4")] private ParamDescriptor Param5 { set => setParam(4, value); }
        [JsonProperty("param_5")] private ParamDescriptor Param6 { set => setParam(5, value); }
        [JsonProperty("param_6")] private ParamDescriptor Param7 { set => setParam(6, value); }
        [JsonProperty("param_7")] private ParamDescriptor Param8 { set => setParam(7, value); }

        // For JSON deserialization
        [JsonConstructor]
        public MapObjMeta() { }

        public MapObjMeta(bool isDocumented)
        {
            IsDocumented = isDocumented;
            Name = "<Unknown MapObj>";
            Description = "<No description provided>";
            // Mark all parameters as used if we don't know this object
            if (!isDocumented)
                Array.ForEach(Params, pd => pd.IsUsed = true);
        }

        /// <summary>
        /// Writes this Meta information to the console for debugging purposes
        /// </summary>
        public void WriteDebugLog()
        {
            foreach (var property in GetType().GetProperties())
            {
                Console.WriteLine("{0} = {1}", property.Name, property.GetValue(this, null));
            }
            Console.Write($"Aliases = {string.Join("|", Usages)}");
            Console.Write($"Usages = {string.Join("|", Usages)}");

            for (int i = 0; i < Params.Length; i++)
            {
                Console.WriteLine($"Param {i}");
                ParamDescriptor pd = Params[i];

                if (pd is null)
                {
                    Console.WriteLine("\tnull");
                    continue;
                }

                foreach (var property in pd.GetType().GetProperties())
                {
                    Console.WriteLine("\t{0} = {1}", property.Name, property.GetValue(pd, null));
                }
                Console.WriteLine("\tEnum: " + string.Join("|", pd.Enum.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
            }
        }

        public void Merge(MapObjMeta other)
        {
            IsDocumented = IsDocumented || other.IsDocumented;
            Name = string.IsNullOrWhiteSpace(other.Name) ? Name : other.Name;
            Description = string.IsNullOrWhiteSpace(other.Description) ? Description : other.Description;
            Aliases.AddRange(other.Aliases);
            Aliases.Sort();
            Usages.AddRange(other.Usages);
            Usages.Sort();
            Platforms.Merge(other.Platforms);

            for (int i = 0; i < Params.Length; i++)
            {
                Params[i].Merge(other.Params[i]);
            }
        }

    }

    /// <summary>
    /// Parameter descriptions
    /// </summary>
    public class ParamDescriptor
    {
        public enum ParamType
        {
            UNKNOWN,
            Float,
            Int,
            Bool,
            Time, // Timer. 60fps, so a value of 60.0 means 1 second.
            Enum,
            Bytes, // Raw bytes
        }

        public bool IsUsed { get; internal set; } = false;

        public int paramIndex { get; set; } = -1;

        private string _name = "";
        public string Name
        {
            get {
                if (!string.IsNullOrWhiteSpace(_name))
                    return _name;
                if (!IsUsed)
                    return string.Format(TranslationSource.GetText("PARAM_UNUSED"), paramIndex);
                return string.Format(TranslationSource.GetText("PARAM_NOTDOC"), paramIndex);
            }
            set => _name = value;
        }

        public string Description { get; set; } = "";

        private float? _default = null;
        public float Default { get => _default ?? 0f; set => _default = value; }

        public List<float> Samples { get; set; } = [];

        public ParamType Type { get; set; } = ParamType.UNKNOWN;

        public float? MinValue { get; set; } = null;
        public float? MaxValue { get; set; } = null;

        public Dictionary<float, string> Enum { get; set; } = [];

        // Wii U/Switch versions
        public MetaPlatforms Platforms { get; } = new();
        [JsonProperty("VersionU")] private UVersion VersionU { set => Platforms.VersionU = value; }
        [JsonProperty("VersionDX")] private DXVersion VersionDX { set => Platforms.VersionDX = value; }

        public ParamDescriptor(int i) {
            paramIndex = i;
        }

        public bool Validate(float value)
        {
            switch (Type)
            {
                case ParamType.Bytes:
                    // Always valid
                    break;
                case ParamType.Float:
                    return ValidateMinMax(value);
                case ParamType.Int:
                    // account for rounding errors
                    return MathF.Abs(value - MathF.Round(value)) <= float.Epsilon && ValidateMinMax(value);
                case ParamType.Time:
                    // account for rounding errors
                    return value >= 0 && MathF.Abs(value - MathF.Round(value)) <= float.Epsilon && ValidateMinMax(value);
                case ParamType.Bool:
                    return value == 0f || value == 1f;
                case ParamType.Enum:
                    return Enum.ContainsKey(value);
            }
            return true;
        }

        private bool ValidateMinMax(float Value)
        {
            return (MinValue is null || Value >= MinValue) && (MaxValue is null || Value <= MaxValue);
        }

        /// <summary>
        /// Merges this descriptor with another one. The other descriptor has priority, assuming values are set.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(ParamDescriptor other)
        {
            IsUsed = IsUsed || other.IsUsed;
            _name = string.IsNullOrWhiteSpace(other._name) ? _name : other._name;
            Description = string.IsNullOrWhiteSpace(other.Description) ? Description : other.Description;
            _default = other._default ?? _default;
            Samples.AddRange(other.Samples);
            Samples.Sort();
            Type = other.Type == ParamType.UNKNOWN ? Type : other.Type;
            MinValue = other.MinValue ?? MinValue;
            MaxValue = other.MaxValue ?? MaxValue;
            foreach (KeyValuePair<float, string> kv in other.Enum) {
                Enum[kv.Key] = kv.Value;
            }
            Platforms.Merge(other.Platforms);
        }
    }
    
}
