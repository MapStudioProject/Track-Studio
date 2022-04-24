using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Toolbox.Core;
using Newtonsoft.Json;

namespace CafeLibrary
{
    public class PluginConfig
    {
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
            if (!File.Exists($"{Runtime.ExecutableDir}\\CafeConfig.json")) { new PluginConfig().Save(); }

            var config = JsonConvert.DeserializeObject<PluginConfig>(File.ReadAllText($"{Runtime.ExecutableDir}\\CafeConfig.json"));
            config.Reload();
            return config;
        }

        /// <summary>
        /// Saves the current configuration to json on disc.
        /// </summary>
        public void Save()
        {
            File.WriteAllText($"{Runtime.ExecutableDir}\\CafeConfig.json", JsonConvert.SerializeObject(this));
            Reload();
        }

        private void Reload()
        {


            init = true;
        }
    }
}
