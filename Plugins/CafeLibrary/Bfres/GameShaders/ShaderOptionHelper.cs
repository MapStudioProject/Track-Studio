using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeLibrary.Rendering
{
    public class ShaderOptionHelper
    {
        /// <summary>
        /// Gets options generated from the render state to be used for program searching.
        /// If a given key is not present as an option, then it will be skipped.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="mat"></param>
        public static void LoadRenderStateOptions(Dictionary<string, string> options, BfresMaterialRender matRender)
        {
            var mat = matRender.Material;
            string renderMode = "";
            if (mat.BlendState.State == GLFrameworkEngine.GLMaterialBlendState.BlendState.Opaque)
                renderMode = "opaque";
            if (mat.BlendState.State == GLFrameworkEngine.GLMaterialBlendState.BlendState.Mask)
                renderMode = "mask";
            if (mat.BlendState.State == GLFrameworkEngine.GLMaterialBlendState.BlendState.Translucent)
                renderMode = "translucent";
            if (mat.BlendState.State == GLFrameworkEngine.GLMaterialBlendState.BlendState.Custom)
                renderMode = "custom";

            if(mat.BlendState.AlphaFunction == OpenTK.Graphics.OpenGL.AlphaFunction.Gequal)
                options["gsys_alpha_test_func"] = "6";
            if (mat.BlendState.AlphaFunction == OpenTK.Graphics.OpenGL.AlphaFunction.Lequal)
                options["gsys_alpha_test_func"] = "3";
            if (mat.BlendState.AlphaFunction == OpenTK.Graphics.OpenGL.AlphaFunction.Less)
                options["gsys_alpha_test_func"] = "1";
            if (mat.BlendState.AlphaFunction == OpenTK.Graphics.OpenGL.AlphaFunction.Never)
                options["gsys_alpha_test_func"] = "0";
            if (mat.BlendState.AlphaFunction == OpenTK.Graphics.OpenGL.AlphaFunction.Greater)
                options["gsys_alpha_test_func"] = "4";


            if (renderMode != null && RenderStateModes.ContainsKey(renderMode))
                options["gsys_renderstate"] = RenderStateModes[renderMode];

            if (mat.BlendState.AlphaTest)
                options["gsys_alpha_test_enable"] = "1";
        }

        static Dictionary<string, string> RenderStateFuncs = new Dictionary<string, string>()
        {
            { "never", "0" },
            { "less", "1" },
            { "lequal", "3" },
            { "greater", "4" },
            { "nequal", "5" },
            { "gequal", "6" },
            { "equal", "7" },
        };

        static Dictionary<string, string> RenderStateModes = new Dictionary<string, string>()
        {
            { "opaque", "0" },
            { "mask", "1" },
            { "translucent", "2" },
            { "custom", "3" },
        };
    }
}
