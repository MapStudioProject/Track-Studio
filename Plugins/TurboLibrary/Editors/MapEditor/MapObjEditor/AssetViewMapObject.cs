using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboLibrary;
using MapStudio.UI;
using ImGuiNET;
using Toolbox.Core;
using MapStudio.UI;

namespace TurboLibrary.MuuntEditor
{
    /// <summary>
    /// Represemts an asset view of map objects to preview, drag and drop objects into the scene.
    /// </summary>
    public class AssetViewMapObject : IAssetLoader
    {
        public virtual string Name => TranslationSource.GetText("Map Objects");

        public bool IsFilterMode => filterObjPath;

        public static bool filterObjPath = false;

        public virtual List<AssetItem> Reload()
        {
            List<AssetItem> assets = new List<AssetItem>();

            var objectList = GlobalSettings.ObjDatabase.Values.ToList();

            assets.Clear();
            foreach (var obj in objectList)
                AddAsset(assets, obj);

            return assets;
        }

        public void AddAsset(List<AssetItem> assets, ObjDefinition obj)
        {
            string resName = obj.ResNames.FirstOrDefault();
            string icon = "Node";
            if (IconManager.HasIcon(System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapObjects",$"{resName}.png")))
                icon = System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapObjects",$"{resName}.png");

            assets.Add(new MapObjectAsset($"MapObject_{obj.ObjId}")
            {
                Name = obj.Label,
                ObjID = obj.ObjId,
                ObjDefinition = obj,
                Icon = IconManager.GetTextureIcon(icon),
            });
        }

        public bool UpdateFilterList()
        {
            bool filterUpdate = false;
            if (ImGui.Checkbox(TranslationSource.GetText("FILTER_PATH_OBJS"), ref filterObjPath))
                filterUpdate = true;

            return filterUpdate;
        }
    }

    public class AssetViewMapObjectVR : AssetViewMapObject
    {
        public override string Name => TranslationSource.GetText("Skyboxes");

        public override List<AssetItem> Reload()
        {
            List<AssetItem> assets = new List<AssetItem>();

            var objectList = GlobalSettings.ObjDatabase.Values.ToList();

            assets.Clear();
            foreach (var obj in objectList)
            {
                if (!obj.VR)
                    continue;

                AddAsset(assets, obj);
            }
            return assets;
        }
    }

    public class MapObjectAsset : AssetItem
    {
        public int ObjID { get; set; }
        public ObjDefinition ObjDefinition { get; set; }

        public override bool Visible
        {
            get
            {
                if (AssetViewMapObject.filterObjPath && ObjDefinition.PathType == (PathType)0)
                    return false;

                return true;
            }
        }

        public override void DoubleClicked()
        {
            string filePath = Obj.FindFilePath(Obj.GetResourceName(ObjID));
            FileUtility.SelectFile(filePath);
        }

        public MapObjectAsset(string id) : base(id)
        {

        }
    }
}
