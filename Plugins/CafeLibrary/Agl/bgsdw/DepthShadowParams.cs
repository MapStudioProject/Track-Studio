using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class DepthShadowParams
    {
        private ParamObject Parent;

        [BindGUI("Light Name", Category = "Depth Shadow")]
        public string LightName
        {
            get { return Parent.GetEntryValue<StringEntry>("light_name").ToString(); }
            set { Parent.SetEntryValue("light_name", new StringEntry(value, 32)); }
        }

        [BindGUI("Light Type", Category = "Depth Shadow")]
        public string LightType
        {
            get { return Parent.GetEntryValue<StringEntry>("light_type").ToString(); }
            set { Parent.SetEntryValue("light_type", new StringEntry(value, 32)); }
        }

        public DepthShadowParams()
        {
            Parent = new ParamObject();
            LightName = "MainLight0";
            LightType = "DirectionalLight";
        }

        public DepthShadowParams(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
