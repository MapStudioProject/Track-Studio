using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class ShadowColorParams
    {
        private ParamObject Parent;

        [BindGUI("Fore Color", Category = "Shadow Color")]
        public STColor ForeColor
        {
            get { return Parent.GetEntryValue<Vector4F>("2148795540").ToSTColor(); }
            set { Parent.SetEntryValue("2148795540", value.ToColorF()); }
        }

        [BindGUI("Back Color", Category = "Shadow Color")]
        public STColor BackColor
        {
            get { return Parent.GetEntryValue<Vector4F>("1496401558").ToSTColor(); }
            set { Parent.SetEntryValue("1496401558", value.ToColorF()); }
        }

        public ShadowColorParams()
        {
            Parent = new ParamObject();
            ForeColor = new STColor(1, 1, 1, 1);
            BackColor = new STColor(0, 0, 0,1);
        }

        public ShadowColorParams(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
