using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class ProjectionShadowParams
    {
        private ParamObject Parent;

        [BindGUI("BiasTrans", Category = "Projection Shadow")]
        public Vector2F BiasTrans
        {
            get { return Parent.GetEntryValue<Vector2F>("bias_trans"); }
            set { Parent.SetEntryValue("bias_trans", value); }
        }

        [BindGUI("BiasScale", Category = "Projection Shadow")]
        public Vector2F BiasScale
        {
            get { return Parent.GetEntryValue<Vector2F>("bias_scale"); }
            set { Parent.SetEntryValue("bias_scale", value); }
        }

        [BindGUI("AnimTransVel", Category = "Projection Shadow")]
        public Vector2F AnimTransVel
        {
            get { return Parent.GetEntryValue<Vector2F>("anim_trans_vel"); }
            set { Parent.SetEntryValue("anim_trans_vel", value); }
        }

        [BindGUI("AnimSwingCycleAmp", Category = "Projection Shadow")]
        public Vector2F AnimSwingCycleAmp
        {
            get { return Parent.GetEntryValue<Vector2F>("anim_swing_amp"); }
            set { Parent.SetEntryValue("anim_swing_amp", value); }
        }

        [BindGUI("AnimSwingCycleX", Category = "Projection Shadow")]
        public float AnimSwingCycleX
        {
            get { return Parent.GetEntryValue<float>("anim_swing_cyc_x"); }
            set { Parent.SetEntryValue("anim_swing_cyc_x", value); }
        }

        [BindGUI("AnimSwingCycleY", Category = "Projection Shadow")]
        public float AnimSwingCycleY
        {
            get { return Parent.GetEntryValue<float>("anim_swing_cyc_y"); }
            set { Parent.SetEntryValue("anim_swing_cyc_y", value); }
        }

        [BindGUI("AnimRotSpeed", Category = "Projection Shadow")]
        public float AnimRotSpeed
        {
            get { return Parent.GetEntryValue<float>("anim_rot_speed"); }
            set { Parent.SetEntryValue("anim_rot_speed", value); }
        }

        [BindGUI("BiasRotate", Category = "Projection Shadow")]
        public float BiasRotate
        {
            get { return Parent.GetEntryValue<float>("anim_rot_speed"); }
            set { Parent.SetEntryValue("anim_rot_speed", value); }
        }

        [BindGUI("Repeat", Category = "Projection Shadow")]
        public bool Repeat
        {
            get { return Parent.GetEntryValue<bool>("repeat"); }
            set { Parent.SetEntryValue("repeat", value); }
        }

        [BindGUI("Name", Category = "Projection Shadow")]
        public string Name
        {
            get { return Parent.GetEntryValue<StringEntry>("proj_name").ToString(); }
            set { Parent.SetEntryValue("proj_name", new StringEntry(value, 32)); }
        }

        [BindGUI("Color", Category = "Projection Shadow")]
        public STColor Color
        {
            get { return Parent.GetEntryValue<Vector4F>("3457397256").ToSTColor(); }
            set { Parent.SetEntryValue("3457397256", value.ToColorF()); }
        }

        [BindGUI("Density", Category = "Projection Shadow")]
        public float Density
        {
            get { return Parent.GetEntryValue<float>("density"); }
            set { Parent.SetEntryValue("density", value); }
        }

        [BindGUI("DensityDrc", Category = "Projection Shadow")]
        public float DensityDrc
        {
            get { return Parent.GetEntryValue<float>("density_drc"); }
            set { Parent.SetEntryValue("density_drc", value); }
        }

        [BindGUI("UseDesnityDrc", Category = "Projection Shadow")]
        public bool UseDesnityDrc
        {
            get { return Parent.GetEntryValue<bool>("use_density_drc"); }
            set { Parent.SetEntryValue("use_density_drc", value); }
        }

        public ProjectionShadowParams()
        {
            Parent = new ParamObject();
            BiasScale = new Vector2F(1, 1);
            AnimTransVel = new Vector2F(0, 0);
            AnimSwingCycleY = 0;
            AnimSwingCycleX = 0;
            AnimSwingCycleAmp = new Vector2F(0, 0);
            BiasTrans = new Vector2F(0, 0.1f);
            AnimRotSpeed = 0.0f;
            BiasRotate = 1.0f;
            Repeat = true;
            Name = "Projection0";
            Color = new STColor(1, 1, 1, 1);
            Density = 0.2f;
            DensityDrc = 1.0f;
            UseDesnityDrc = false;
        }

        public ProjectionShadowParams(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
