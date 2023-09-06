using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Toolbox.Core;
using Newtonsoft.Json;
using MapStudio.UI;
using ImGuiNET;

namespace CafeLibrary
{
    public class PluginConfig : IPluginConfig
    {
        [JsonProperty]
        public static string TotkGamePath = "";

        public static bool IsValidTotkGamePath;


        [JsonProperty]
        public static string MaterialPreset = "";

        [JsonProperty]
        public static bool UseGameShaders = false;

        public static bool init = false;

        public static PluginConfig Instance = new PluginConfig();

        /// <summary>
        /// Loads the config json file on disc or creates a new one if it does not exist.
        /// </summary>
        /// <returns></returns>
        public static PluginConfig Load()
        {
            if (!File.Exists(Path.Combine(Runtime.ExecutableDir,"CafeConfig.json"))) { new PluginConfig().Save(); }

            var config = JsonConvert.DeserializeObject<PluginConfig>(File.ReadAllText(Path.Combine(Runtime.ExecutableDir,"CafeConfig.json")));
            config.Reload();
            return config;
        }

        /// <summary>
        /// Renders the current configuration UI.
        /// </summary>
        public void DrawUI()
        {
            if (ImguiCustomWidgets.PathSelector("TOTK Game Path", ref TotkGamePath, IsValidTotkGamePath))
            {
                ZDic.DumpExternalDictionaries();
                Save();
            }
            if (IsValidTotkGamePath)
            {
                if (ImGui.Button("Dump ZS Dictionaries"))
                {
                    ZDic.DumpExternalDictionaries();
                }
            }
        }

        /// <summary>
        /// Saves the current configuration to json on disc.
        /// </summary>
        public void Save()
        {
            File.WriteAllText(Path.Combine(Runtime.ExecutableDir,"CafeConfig.json"), JsonConvert.SerializeObject(this));
            Reload();
        }

        private void Reload()
        {
            IsValidTotkGamePath = File.Exists(Path.Combine(TotkGamePath, "Shader", "ExternalBinaryString.bfres.mc"));
            init = true;
        }
    }
}
