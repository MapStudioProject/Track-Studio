using System;
using System.Collections.Generic;
using System.Linq;
using MapStudio.UI;
using Newtonsoft.Json;

namespace TurboLibrary
{
    /// <summary>
    /// Meta information for an object (<see cref="Obj"/>) placed in the course. This information is not present
    /// in the <c>objflow.byaml</c>, but serves as more user-friendly guidelines for editing.
    /// </summary>
    public class MapObjMeta
    {
        private static readonly string DefaultName = "<Map Object>";
        private static readonly string DefaultDescription = "<No description provided>";

        public bool IsDocumented { get; set; } = true;

        public string Name { get; set; } = DefaultName;

        public string Description { get; set; } = DefaultDescription;

        public string[] Aliases { get; set; } = [];

        public string[] Usages { get; set; } = []; // "U Sunshine Airport, ..."

        public string[] DLCRequiredU { get; set; } = [];
        public string[] DLCRequiredDX { get; set; } = [];

        public ParamDescriptor[] Params { get; } = [
            new ParamDescriptor(0), new ParamDescriptor(1), new ParamDescriptor(2), new ParamDescriptor(3),
            new ParamDescriptor(4), new ParamDescriptor(5), new ParamDescriptor(6), new ParamDescriptor(7)
        ];

        // For deserialization. The JSON format is more user friendly when allowing parameters to be defined this way.
        [JsonProperty("param_0")] private ParamDescriptor Param1 { set { Params[0] = value; Params[0].IsUsed = true; } }
        [JsonProperty("param_1")] private ParamDescriptor Param2 { set { Params[1] = value; Params[1].IsUsed = true; } }
        [JsonProperty("param_2")] private ParamDescriptor Param3 { set { Params[2] = value; Params[2].IsUsed = true; } }
        [JsonProperty("param_3")] private ParamDescriptor Param4 { set { Params[3] = value; Params[3].IsUsed = true; } }
        [JsonProperty("param_4")] private ParamDescriptor Param5 { set { Params[4] = value; Params[4].IsUsed = true; } }
        [JsonProperty("param_5")] private ParamDescriptor Param6 { set { Params[5] = value; Params[5].IsUsed = true; } }
        [JsonProperty("param_6")] private ParamDescriptor Param7 { set { Params[6] = value; Params[6].IsUsed = true; } }
        [JsonProperty("param_7")] private ParamDescriptor Param8 { set { Params[7] = value; Params[7].IsUsed = true; } }

        public MapObjMeta(bool isDocumented)
        {
            IsDocumented = IsDocumented;
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
            Name = other.Name == DefaultName ? Name : other.Name;
            Description = other.Description == DefaultDescription ? Description : other.Description;
            Aliases.Union(other.Aliases);
            Array.Sort(Aliases);
            Usages.Union(other.Usages);
            Array.Sort(Usages);
            DLCRequiredU.Union(other.DLCRequiredU);
            Array.Sort(DLCRequiredU);
            DLCRequiredDX.Union(other.DLCRequiredDX);
            Array.Sort(DLCRequiredDX);

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
        private static readonly string DefaultName = "<Param Name>";
        private static readonly string DefaultDescription = "<Param Description>";


        public enum ParamType
        {
            UNKNOWN,
            Float,
            Int,
            Bool,
            Frame, // Timer. 60fps, so a value of 60.0 means 1 second.
            Enum,
            Bytes, // Raw bytes
        }

        public bool IsUsed { get; set; } = false;

        public string Name { get; set; } = DefaultName;

        public string Description { get; set; } = DefaultDescription;

        public float Default { get; set; } = 0f;

        public float[] Samples { get; set; } = [];

        public bool IsSupportU { get; set; } = true; // Parameter is used in the Wii U version
        public bool IsSupportDX { get; set; } = true; // Parameter is used in Deluxe on the Switch
        public bool IsModded { get; set; } = false; // Parameter added by a code mod

        public ParamType Type { get; set; } = ParamType.UNKNOWN;

        public float? MinValue { get; set; } = null;
        public float? MaxValue { get; set; } = null;

        public Dictionary<float, string> Enum { get; set; } = [];

        public ParamDescriptor(int i) {
            Name = string.Format(TranslationSource.GetText("UNUSED"), i);
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
                case ParamType.Frame:
                    // account for rounding errors
                    return MathF.Abs(value - MathF.Round(value)) <= float.Epsilon && ValidateMinMax(value);
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
            Name = other.Name == DefaultName ? Name : other.Name;
            Description = other.Description == DefaultDescription ? Description : other.Description;
            Default = other.Default == 0f ? Default : other.Default;
            Samples.Union(other.Samples);
            Array.Sort(Samples);
            IsSupportU = IsSupportU && other.IsSupportU;
            IsSupportDX = IsSupportDX && other.IsSupportDX;
            IsModded = IsModded || other.IsModded;
            Type = other.Type == ParamType.UNKNOWN ? Type : other.Type;
            MinValue = other.MinValue ?? MinValue;
            MaxValue = other.MaxValue ?? MaxValue;
            foreach (KeyValuePair<float, string> kv in other.Enum) {
                Enum[kv.Key] = kv.Value;
            }
        }
    }
    
}
