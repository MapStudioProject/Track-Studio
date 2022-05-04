using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;
using Newtonsoft.Json;
using ImGuiNET;
using MapStudio.UI;
using Toolbox.Core;

namespace TurboLibrary
{
    public class PluginConfig : IPluginConfig
    {
        //Only load the config once when this constructor is activated.
        internal static PluginConfig Instance = null;

        internal static bool Init => Instance != null;

        public PluginConfig() {
            if (Instance == null) Instance = this;
        }

        [JsonProperty]
        public static string MK8DGamePath = "";

        [JsonProperty]
        public static string MK8DUpdatePath = "";

        [JsonProperty]
        public static string MK8AOCPath = "";

        [JsonProperty]
        public static string MK8ModPath = "";

        [JsonIgnore]
        static bool HasValidMK8DPath = false;

        [JsonIgnore]
        static bool HasValidMK8UpdatePath = false;

        [JsonIgnore]
        static bool HasValidMK8AOCPath = false;

        /// <summary>
        /// Renders the current configuration UI.
        /// </summary>
        public void DrawUI()
        {
            if (ImguiCustomWidgets.PathSelector("Mario Kart 8 Path", ref MK8DGamePath, HasValidMK8DPath))
            {
                Save();
            }
            if (ImguiCustomWidgets.PathSelector("Mario Kart 8 Update Path", ref MK8DUpdatePath, HasValidMK8UpdatePath))
            {
                Save();
            }
            if (ImguiCustomWidgets.PathSelector("Mario Kart 8 DLC Path", ref MK8AOCPath, HasValidMK8AOCPath))
            {
                Save();
            }
            if (ImguiCustomWidgets.PathSelector("Mario Kart 8 Mod Path", ref MK8ModPath, !string.IsNullOrEmpty(MK8ModPath)))
            {
                Save();
            }
        }

        /// <summary>
        /// Loads the config json file on disc or creates a new one if it does not exist.
        /// </summary>
        /// <returns></returns>
        public static PluginConfig Load() {
            if (!File.Exists($"{Runtime.ExecutableDir}\\TurboConfig.json")) { new PluginConfig().Save(); }

            var config = JsonConvert.DeserializeObject<PluginConfig>(File.ReadAllText($"{Runtime.ExecutableDir}\\TurboConfig.json"));
            config.Reload();
            return config;
        }

        /// <summary>
        /// Saves the current configuration to json on disc.
        /// </summary>
        public void Save() {
           File.WriteAllText($"{Runtime.ExecutableDir}\\TurboConfig.json", JsonConvert.SerializeObject(this));
            Reload();
        }

        /// <summary>
        /// Called when the config file has been loaded or saved.
        /// </summary>
        public void Reload()
        {
            TurboLibrary.GlobalSettings.GamePath = MK8DGamePath;
            HasValidMK8DPath = File.Exists($"{MK8DGamePath}\\Data\\objflow.byaml");

            TurboLibrary.GlobalSettings.UpdatePath = MK8DUpdatePath;
            HasValidMK8UpdatePath = File.Exists($"{MK8DUpdatePath}\\Data\\objflow.byaml");

            TurboLibrary.GlobalSettings.AOCPath = MK8AOCPath;
            HasValidMK8AOCPath = Directory.Exists($"{MK8AOCPath}\\0013\\course"); //Just check one of the dlc folders

            TurboLibrary.GlobalSettings.ModOutputPath = MK8ModPath;

            //Check presets
            CafeLibrary.MaterialPresetDumper.ExportMaterials(MK8DGamePath);
        }
    }
}
