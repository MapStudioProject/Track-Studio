using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.MuuntEditor
{
    public class StartObjRender : EditableObject, IColorPickable
    {
        public bool IsLeft => MapLoader.Instance.CourseDefinition.IsFirstLeft == true;

        Vector4 Color = new Vector4(1);

        StartGridRenderer StartGrid = null;
        OrientationCubeDrawer CenterPoint = null;

        public override BoundingNode BoundingNode { get; } = new BoundingNode()
        {
            Box = new BoundingBox(
                       new OpenTK.Vector3(-10, -10, -10),
                       new OpenTK.Vector3(10, 10, 10)),
        };

        public StartObjRender(NodeBase parent) : base(parent)
        {
        }

        public void DrawColorPicking(GLContext context)
        {
            Prepare();

            StartGrid.DrawPicking(context, this, Transform.TransformMatrix);
            CenterPoint.DrawPicking(context, this, Transform.TransformMatrix);

            GL.LineWidth(1);
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE)
                return;

            Prepare();

            GL.Disable(EnableCap.CullFace);

            CenterPoint.DrawModel(context, Transform.TransformMatrix, IsSelected || IsHovered);

            var mat = Matrix4.CreateTranslation(0, 1, 0) * Transform.TransformMatrix;
            if (IsLeft)
                mat = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(180)) * mat;

            StartGrid.DrawSolidWithSelection(context, mat, Color, IsSelected || IsHovered);

            GL.Enable(EnableCap.CullFace);
        }

        private void Prepare()
        {
            if (StartGrid == null)
                StartGrid = new StartGridRenderer();
            if (CenterPoint == null)
                CenterPoint = new OrientationCubeDrawer();
        }

        public override void Dispose()
        {
            CenterPoint?.Dispose();
            StartGrid?.Dispose();
        }

        class StartGridRenderer : RenderMesh<VertexPositionNormalTexCoord>
        {
            public StartGridRenderer(float size = 1.0f, PrimitiveType primitiveType = PrimitiveType.Triangles)
            {
                var obj = DrawingHelper.FromObj(Properties.Resources.StartGrid);
                Init(obj.Item1.ToArray(), obj.Item2);
                UpdatePrimitiveType(primitiveType);
            }
        }
    }
}
