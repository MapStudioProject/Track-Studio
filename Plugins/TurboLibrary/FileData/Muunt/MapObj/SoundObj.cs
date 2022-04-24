using System;
using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a region in which a sound is emitted.
    /// </summary>
    [ByamlObject]
    public class SoundObj : SpatialObject, IByamlSerializable
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the game modes in which this Obj will appear.
        /// </summary>
        public ModeInclusion ModeInclusion { get; set; }

        [BindGUI("Multi2P", Category = "Object", ColumnIndex = 0, Control = BindControl.ToggleButton)]
        public bool HasMulti2P
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.Multi2P); }
            set
            {
                if (value)
                    ModeInclusion |= ModeInclusion.Multi2P;
                else
                    ModeInclusion &= ~ModeInclusion.Multi2P;
            }
        }

        [BindGUI("Multi4P", Category = "Object", ColumnIndex = 1, Control = BindControl.ToggleButton)]
        public bool HasMulti4P
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.Multi4P); }
            set
            {
                if (value)
                    ModeInclusion |= ModeInclusion.Multi4P;
                else
                    ModeInclusion &= ~ModeInclusion.Multi4P;
            }
        }

        [BindGUI("WiFi", Category = "Object", ColumnIndex = 2, Control = BindControl.ToggleButton)]
        public bool HasWiFi
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.WiFi); }
            set
            {
                if (value)
                    ModeInclusion |= ModeInclusion.WiFi;
                else
                    ModeInclusion &= ~ModeInclusion.WiFi;
            }
        }

        [BindGUI("WiFi2P", Category = "Object", ColumnIndex = 3, Control = BindControl.ToggleButton)]
        public bool HasWiFi2P
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.WiFi2P); }
            set
            {
                if (value)
                    ModeInclusion |= ModeInclusion.WiFi2P;
                else
                    ModeInclusion &= ~ModeInclusion.WiFi2P;
            }
        }

        /// <summary>
        /// Gets or sets an unknown setting.
        /// </summary>
        [BindGUI("Single", Category = "Object")]
        public bool? Single { get; set; }

        /// <summary>
        /// Gets or sets an unknown setting which has never been used in the original courses.
        /// </summary>
        [BindGUI("Top View", Category = "Object")]
        public bool TopView { get; set; }

        /// <summary>
        /// Gets or sets the first parameter.
        /// </summary>
        [ByamlMember("prm1")]
        [BindGUI("Sound ID", Category = "Params", ColumnIndex = 0)]
        public int Prm1 { get; set; }

        /// <summary>
        /// Gets or sets the second parameter.
        /// </summary>
        [ByamlMember("prm2")]
        [BindGUI("Param 2", Category = "Params", ColumnIndex = 1)]
        public int Prm2 { get; set; }

        public SoundObj()
        {
            HasMulti2P = true;
            HasMulti4P = true;
            HasWiFi = true;
            HasWiFi2P = true;
            Prm1 = 0;
            Prm2 = 0;
            Scale = new ByamlVector3F(1, 1, 1);
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Reads data from the given <paramref name="dictionary"/> to satisfy members.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{String, Object}"/> to read the data from.</param>
        public void DeserializeByaml(IDictionary<string, object> dictionary)
        {
            TopView = (string)dictionary["TopView"] == "True";
            ModeInclusion = ModeInclusion.FromDictionary(dictionary);

            if (dictionary.TryGetValue("Single", out object single)) Single = (string)single == "true";
        }

        /// <summary>
        /// Writes instance members into the given <paramref name="dictionary"/> to store them as BYAML data.
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{String, Object}"/> to store the data in.</param>
        public void SerializeByaml(IDictionary<string, object> dictionary)
        {
            dictionary["TopView"] = TopView ? "True" : "False";
            ModeInclusion.ToDictionary(dictionary);

            if (Single.HasValue) dictionary["Single"] = Single.Value ? "true" : "false";
        }
    }
}
