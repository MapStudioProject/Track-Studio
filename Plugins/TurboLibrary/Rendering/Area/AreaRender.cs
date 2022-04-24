using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.ViewModels;

namespace CafeLibrary.Rendering
{
    public class AreaRender : EditableObject, IColorPickable
    {
        public static bool DrawFilled = false;
        public static float Transparency = 0.1f;

        public Vector4 Color = Vector4.One;
        public Vector4 FillColor = new Vector4(0.4f, 0.7f, 1.0f, 0.3f);

        public bool IsSphere = false;

        CubeCrossedRenderer CubeOutlineRender = null;
        CubeRenderer CubeFilledRenderer = null;

        SphereRender SphereOutlineRender = null;
        SphereRender SphereFilledRenderer = null;

        //Area boxes have an inital transform
        static Matrix4 InitalTransform => new Matrix4(
            50, 0, 0, 0,
            0, 50, 0, 0,
            0, 0, 50, 0,
            0, 50, 0, 1);

        public AreaRender(NodeBase parent, Vector4 color) : base(parent)
        {
            Color = color;
        }

        public override BoundingNode BoundingNode { get; } = new BoundingNode()
        {
            Box = new BoundingBox(
                new OpenTK.Vector3(-100, -100, -50),
                new OpenTK.Vector3(100, 100, 150)),
        };

        public void DrawColorPicking(GLContext context)
        {
            Prepare();

            //Thicker picking region
            GL.LineWidth(32);
            if (IsSphere)
                SphereOutlineRender.DrawPicking(context, this, InitalTransform * Transform.TransformMatrix);
            else
                CubeOutlineRender.DrawPicking(context, this, InitalTransform * Transform.TransformMatrix);
            GL.LineWidth(1);
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE)
                return;

            var matrix = InitalTransform * Transform.TransformMatrix;

            Prepare();

            GL.Disable(EnableCap.CullFace);

            //Draw a filled in region
            if (DrawFilled)
            {
                GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();
                GLMaterialBlendState.TranslucentAlphaOne.RenderDepthTest();
                if (IsSphere)
                    SphereFilledRenderer.DrawSolid(context, matrix, new Vector4(Color.Xyz, Transparency));
                else
                    CubeFilledRenderer.DrawSolid(context, matrix, new Vector4(Color.Xyz, Transparency));
                GLMaterialBlendState.Opaque.RenderBlendState();
                GLMaterialBlendState.Opaque.RenderDepthTest();
            }

            //Draw lines of the region
            GL.LineWidth(4);
            if (IsSphere)
                SphereOutlineRender.DrawSolidWithSelection(context, matrix, Color, IsSelected | IsHovered);
            else
                CubeOutlineRender.DrawSolidWithSelection(context, matrix, Color, IsSelected | IsHovered);
            GL.LineWidth(1);

            GL.Enable(EnableCap.CullFace);
        }

        private void Prepare()
        {
            if (IsSphere)
            {
                if (SphereOutlineRender == null)
                    SphereOutlineRender = new SphereRender(0.5f, 10, 10, PrimitiveType.LineStrip);
                if (SphereFilledRenderer == null)
                    SphereFilledRenderer = new SphereRender(0.5f, 10, 10);
            }
            else
            {
                if (CubeOutlineRender == null)
                    CubeOutlineRender = new CubeCrossedRenderer(1, PrimitiveType.LineStrip);
                if (CubeFilledRenderer == null)
                    CubeFilledRenderer = new CubeRenderer(1);
            }
        }

        public override void Dispose()
        {
            CubeOutlineRender?.Dispose();
            CubeFilledRenderer?.Dispose();

            SphereOutlineRender?.Dispose();
            SphereFilledRenderer?.Dispose();
        }

        class CubeCrossedRenderer : RenderMesh<VertexPositionNormal>
        {
            public CubeCrossedRenderer(float size = 1.0f, PrimitiveType primitiveType = PrimitiveType.Triangles) :
                base(DrawingHelper.GetCubeVertices(size), Indices, primitiveType)
            {

            }

            public static int[] Indices = new int[]
            {
            // front face
            0, 1, 2, 2, 3, 0,
            // top face
            3, 2, 6, 6, 7, 3,
            // back face
            7, 6, 5, 5, 4, 7,
            // left face
            4, 0, 3, 3, 7, 4,
            // bottom face
            0, 5, 1, 4, 5, 0, //Here we swap some indices for a cross section at the bottom
            // right face
            1, 5, 6, 6, 2, 1,};
        }
    }
}
