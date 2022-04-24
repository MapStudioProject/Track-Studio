using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using GLFrameworkEngine;
using MapStudio.UI;

namespace CafeLibrary
{
    public class Plugin : IPlugin
    {
        public string Name => "Cafe Editor";

        public Plugin()
        {
           // UIManager.Subscribe(UIManager.UI_TYPE.NEW_FILE, "Bfres File", typeof(BFRES));

            //Load plugin specific data. This is where the game path is stored.
            if (!PluginConfig.init)
                PluginConfig.Instance = PluginConfig.Load();
        }
    }
}