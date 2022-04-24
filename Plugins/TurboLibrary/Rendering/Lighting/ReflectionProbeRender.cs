using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;

namespace CafeLibrary.Rendering
{
    public class ReflectionProbeRender : ITransformableObject, IRayCastPicking, IDrawable
    {
        public bool IsVisible { get; set; } = true;

        public GLTransform Transform { get; set; } = new GLTransform();

        public bool IsHovered { get; set; }
        public bool IsSelected { get; set; }
        public bool CanSelect { get; set; } = true;

        public BoundingNode GetRayBounding() => new BoundingNode() { Radius = 20};
        
        public UVSphereRender SphereRender;

        StandardMaterial Material = new StandardMaterial();

        public void DrawModel(GLContext context, Pass pass)
        {
            if (SphereRender == null)
                SphereRender = new UVSphereRender();

            Material.Render(context);
            SphereRender.Draw(context);
        }

        public void Dispose()
        {
            SphereRender?.Dispose();
        }


    }
}
