using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp;

namespace AGraphicsLibrary
{
    public class ShadowGraphics
    {
        public List<DepthShadowParams> DepthParams = new List<DepthShadowParams>();
        public List<ProjectionShadowParams> ProjectionParams = new List<ProjectionShadowParams>();

        public ShadowColorParams ShadowColors { get; set; }
        public StaticDepthShadowParams StaticDepthShadowParams { get; set; }

        public ShadowGraphics()
        {
            DepthParams.Add(new DepthShadowParams());
            ProjectionParams.Add(new ProjectionShadowParams());
            ShadowColors = new ShadowColorParams();
            StaticDepthShadowParams = new StaticDepthShadowParams();
        }

        public ShadowGraphics(AampFile aamp)
        {
            foreach (var obj in aamp.RootNode.paramObjects)
            {
                if (obj.HashString == "4137089673")
                    ShadowColors = new ShadowColorParams(obj);
                else if (obj.HashString.Contains("depth_shadow_parameter_"))
                    DepthParams.Add(new DepthShadowParams(obj));
                else if (obj.HashString.Contains("static_depth_shadow_parameter"))
                    StaticDepthShadowParams = new StaticDepthShadowParams(obj);
                else if (obj.HashString.Contains("projection_shadow_"))
                    ProjectionParams.Add(new ProjectionShadowParams(obj));
            }
        }
    }
}
