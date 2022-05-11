using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using TurboLibrary;
using MapStudio.UI;
using GLFrameworkEngine;

namespace TurboLibrary
{
    public class Plugin : IPlugin
    {
        public string Name => "MK8 Map Editor";

        public Plugin()
        {
            CafeLibrary.Rendering.BfresLoader.TargetShader = typeof(CafeLibrary.Rendering.TurboNXRender);
            UIManager.Subscribe(UIManager.UI_TYPE.NEW_FILE, "Custom Track", typeof(CourseMuuntPlugin));
            //UIManager.Subscribe(UIManager.UI_TYPE.NEW_FILE, "Kcl File", typeof(KclPlugin));

            //Load plugin specific data. This is where the game path is stored.
            if (!PluginConfig.Init)
                PluginConfig.Load();

            GlobalShaders.AddShader("KCL", System.IO.Path.Combine("KCL","CollisionDefault"));
            GlobalShaders.AddShader("MINIMAP", System.IO.Path.Combine("Turbo","MinimapFilter"));
        }
    }
}
