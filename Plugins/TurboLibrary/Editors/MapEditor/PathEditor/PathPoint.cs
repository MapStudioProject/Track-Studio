using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GLFrameworkEngine;
using TurboLibrary;
using OpenTK.Graphics.OpenGL;

namespace TurboLibrary.MuuntEditor
{
    public class PathPoint<TPath, TPoint> : RenderablePathPoint
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        public Type PointType => typeof(TPoint);
        public Type GroupType => typeof(TPath);

        public TPoint Tag { get; set; }
        public TPath Group { get; set; }

        public override BoundingNode GetRayBounding()
        {
            var bounding = base.GetRayBounding();
            bounding.TransformByScale = false;
            return bounding;
        }

        public PathPoint(RenderablePath path, Vector3 position)
                 : base(path, position)
        {
            CreateNewObject();
        }

        public PathPoint(RenderablePath path, Vector3 color, Vector3 pos,
            Vector3 rot, Vector3 sca) : base(path, color)
        {
            CreateNewObject();
        }

        public override void Render(GLContext context, Pass pass)
        {
            if (SphereRender == null)
                Init();

            if (ParentPath.InterpolationMode == RenderablePath.Interpolation.Bezier)
            {
                if (pass == Pass.OPAQUE)
                    base.Render(context, pass);
            }
            else
            {
                //Scale from camera position
                CameraScale = context.Camera.ScaleByCameraDistance(Transform.Position);
                RaySphereSize = 10 * RenderablePath.PointSize;

                var matrix = Matrix4.CreateScale(RaySphereSize * CameraScale) * Transform.TransformNoScaleMatrix;

                Matrix4 boundingTransform = Matrix4.CreateScale(0.5f) * Transform.TransformMatrix;

                if (pass == Pass.OPAQUE)
                {
                    SphereRender.DrawSolidWithSelection(context, matrix,
                         ParentPath.PointColor, IsSelected || IsHovered);
                }
                if (pass == Pass.TRANSPARENT)
                {
                    if (RenderablePath.DisplayPointSize)
                    {
                        DrawSizeSphere(context, boundingTransform, Pass.TRANSPARENT);
                    }
                }
            }
        }

        public virtual void DrawSizeSphere(GLContext context, Matrix4 matrix, Pass pass)
        {
            GLMaterialBlendState.Translucent.RenderBlendState();

            Vector4 blockColor = new Vector4(1, 1, 0, 0.2f);
            SphereRender.DrawSolid(context, matrix, blockColor);

            GLMaterialBlendState.Opaque.RenderBlendState();
        }

        public virtual void DrawSizeCircle(GLContext context, Matrix4 matrix, Pass pass)
        {
            GLMaterialBlendState.Translucent.RenderBlendState();

            Vector4 blockColor = new Vector4(1, 1, 0, 0.2f);

            GL.Enable(EnableCap.StencilTest);
            GL.StencilFunc(StencilFunction.Always, 0, 0xff);
            GL.StencilMask(0xff);
            GL.ClearStencil(0);
            GL.Clear(ClearBufferMask.StencilBufferBit);

            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.CullFace(CullFaceMode.Front);
            GL.StencilOp(StencilOp.Keep, StencilOp.Incr, StencilOp.Keep);
            SphereRender.DrawSolid(context, matrix, blockColor);

            GL.CullFace(CullFaceMode.Back);
            GL.StencilOp(StencilOp.Keep, StencilOp.Decr, StencilOp.Keep);
            SphereRender.DrawSolid(context, matrix, blockColor);

            GL.CullFace(CullFaceMode.Back);
            GL.ColorMask(true, true, true, true);
            GL.StencilMask(0x00);
            GL.StencilFunc(StencilFunction.Notequal, 0, 0xff);
            SphereRender.DrawSolid(context, matrix, blockColor);

            GL.DepthMask(true);
            GL.StencilMask(0xff);
            GL.Disable(EnableCap.StencilTest);

            GLMaterialBlendState.Opaque.RenderBlendState();
        }

        private void CreateNewObject()
        {
            Tag = (TPoint)Activator.CreateInstance(PointType);
            if (Tag.Scale.HasValue)
                Transform.Scale = new Vector3(
                    Tag.Scale.Value.X,
                    Tag.Scale.Value.Y, 
                    Tag.Scale.Value.Z) * GLContext.PreviewScale;

            if (typeof(TPath) ==  typeof(GlidePath))
                ParentPath.DefaultScale = new Vector3(200, 200, 200);
            else
                ParentPath.DefaultScale = new Vector3(200, 200, 200);   

            Transform.UpdateMatrix(true);
        }

        private static bool assignGroups = true;

        public override void ConnectToPoint(RenderablePathPoint point)
        {
            List<RenderablePathPoint> parents = new List<RenderablePathPoint>();
            List<RenderablePathPoint> children = new List<RenderablePathPoint>();

            foreach (var pt in Parents)
                parents.Add(pt);

            foreach (var pt in Children)
                children.Add(pt);

            assignGroups = false;

            //Make sure the group's hover over tag stays the same
            foreach (var child in children)
                point.AddChild(child);

            foreach (var parent in parents)
                parent.AddChild(point);

            foreach (var parent in parents)
                parent.Children.Remove(this);

            foreach (var child in children)
                child.Parents.Remove(this);

            ParentPath.RemovePoint(this);

            assignGroups = true;
        }

        public void AssignParentGroup(RenderablePathPoint point) {
            Tag.Path = ((PathPoint<TPath, TPoint>)point).Tag.Path;
            Group = Tag.Path;
        }

        public void CreateGroup() {
            Tag.Path = (TPath)Activator.CreateInstance(GroupType);
            Group = Tag.Path;
        }

        public virtual void OnGroupUpdated()
        {

        }
    }
}
