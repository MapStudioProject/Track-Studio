using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ryujinx.Common.Logging;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Meta information for an object (<see cref="Obj"/>) placed in the course. This information is not present
    /// in the <c>objflow.byaml</c>, but serves as more user-friendly guidelines for editing.
    /// </summary>
    public class MapObjMeta
    {
        public string Name { get; set; } = "<Map Object>";

        public string Description { get; set; } = "<No Description provided>";

        public string[] Aliases { get; set; } = [];

        public string[] Usages { get; set; } = []; // "U Sunshine Airport, ..."

        public string[] DLCRequiredU { get; set; } = [];
        public string[] DLCRequiredDX { get; set; } = [];

        public ParamDescriptor[] Params { get; } = [null, null, null, null, null, null, null, null];

        // For deserialization. The JSON format is more user friendly when allowing parameters to be defined this way.
        [JsonProperty("param_0")] private ParamDescriptor Param1 { set { Params[0] = value; } }
        [JsonProperty("param_1")] private ParamDescriptor Param2 { set { Params[1] = value; } }
        [JsonProperty("param_2")] private ParamDescriptor Param3 { set { Params[2] = value; } }
        [JsonProperty("param_3")] private ParamDescriptor Param4 { set { Params[3] = value; } }
        [JsonProperty("param_4")] private ParamDescriptor Param5 { set { Params[4] = value; } }
        [JsonProperty("param_5")] private ParamDescriptor Param6 { set { Params[5] = value; } }
        [JsonProperty("param_6")] private ParamDescriptor Param7 { set { Params[6] = value; } }
        [JsonProperty("param_7")] private ParamDescriptor Param8 { set { Params[7] = value; } }

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

    }

    /// <summary>
    /// Parameter descriptions
    /// </summary>
    public class ParamDescriptor
    {
        public enum ParamType
        {
            INVALID,
            UNKNOWN,
            Float,
            Int,
            Bool,
            Frame, // Timer. 60fps, so a value of 60.0 means 1 second.
            Enum,
            Bytes, // Raw bytes
        }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public float Default { get; set; } = 0f;

        public bool IsSupportU { get; set; } = true; // Parameter is used in the Wii U version
        public bool IsSupportDX { get; set; } = true; // Parameter is used in Deluxe on the Switch
        public bool IsModded { get; set; } = false; // Parameter added by a code mod

        public ParamType Type { get; set; } = ParamType.UNKNOWN;

        public float? MinValue { get; set; } = null;
        public float? MaxValue { get; set; } = null;

        public Dictionary<float, string> Enum { get; set; } = [];

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


    }
    
}
