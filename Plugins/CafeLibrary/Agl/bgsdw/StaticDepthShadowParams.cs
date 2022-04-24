using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class StaticDepthShadowParams
    {
        private ParamObject Parent;

        [BindGUI("Light Name", Category = "Static Depth")]
        public string LightName
        {
            get { return Parent.GetEntryValue<StringEntry>("light_name").ToString(); }
            set { Parent.SetEntryValue("light_name", new StringEntry(value, 32)); }
        }

        public StaticDepthShadowParams()
        {
            Parent = new ParamObject();
            LightName = "MainLight0";
        }

        public StaticDepthShadowParams(ParamObject paramObject)
        {
            Parent = paramObject;
        }
    }
}
