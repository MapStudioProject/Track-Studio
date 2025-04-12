using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using TurboLibrary;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.MuuntEditor
{
    public class CubePathRender<TPath, TPoint> : PathRender<TPath, TPoint>
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        public override bool EditMode => true;

        public bool InPathMode = false;

        StandardMaterial LineMaterial = new StandardMaterial();
        ConeRenderer ConeRenderer;

        public override IEnumerable<ITransformableObject> Selectables
        {
            get
            {
                List<ITransformableObject> objects = new List<ITransformableObject>();
                for (int i = 0; i < PathPoints.Count; i++)
                {
                    var point = ((CubePathPoint<TPath, TPoint>)PathPoints[i]);
                    objects.Add(point);
                    if (point.ReturnPoint != null)
                        objects.Add(point.ReturnPoint);

                }
                return objects;
            }
        }

        public CubePathRender(List<PathBase<TPath, TPoint>> groups, NodeBase node) : base(groups, node)
        {
            ConnectHoveredPoints = false;
            PathUITagType = typeof(TPath);
            PointUITagType = typeof(TPoint);
            LineWidth = 3;
        }

        public override void DrawColorPicking(GLContext context)
        {
            if (InPathMode)
                return;

            foreach (CubePathPoint<TPath, TPoint> pathPoint in PathPoints) {
                pathPoint.DrawColorPicking(context);
            }
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            if (XRayMode)
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Enable(EnableCap.DepthTest);
            }

            foreach (var pathPoint in PathPoints) {
                if (!pathPoint.IsVisible)
                    continue;

                pathPoint.Render(context, pass);
            }

            if (pass == Pass.OPAQUE) {
                DrawLineDisplay(context);
                if (InPathMode)
                {
                    DrawArrowDisplay(context);
                   // DrawCubeBounary(context);
                }
            }

            if (XRayMode)
                GL.Enable(EnableCap.DepthTest);
        }
/*
        public override void RemovePoint(RenderablePathPoint point)
        {
            base.RemovePoint(point);

            var pt = ((CubePathPoint<TPath, TPoint>)point);
            var groupNode = NodeFolder.Children.FirstOrDefault(x => x.Tag == pt.Group);
            if (groupNode == null) return;

            groupNode.Children.Remove(pt.UINode);
            if (groupNode.Children.Count == 0)
                NodeFolder.Children.Remove(groupNode);
        }*/

        public override RenderablePathPoint CreatePoint(Vector3 position)
        {
            var point = new CubePathPoint<TPath, TPoint>(this, position);
            point.Transform.UpdateMatrix(true);
            return point;
        }

        CubeRenderer Cube;

        public void DrawCubeBounary(GLContext context, bool picking = false)
        {
            foreach (CubePathPoint<TPath, TPoint> point in PathPoints)
            {
                if (!point.IsVisible)
                    continue;

                foreach (CubePathPoint<TPath, TPoint> nextPt in point.Children)
                {
                    if (Cube == null)
                        Cube = new CubeRenderer(1, PrimitiveType.TriangleStrip);

                    List<VertexPositionNormal> points = new List<VertexPositionNormal>();
                    points.Add(new VertexPositionNormal(point.CornerPoints[0], Vector3.Zero));
                    points.Add(new VertexPositionNormal(point.CornerPoints[1], Vector3.Zero));
                    points.Add(new VertexPositionNormal(nextPt.CornerPoints[0], Vector3.Zero));
                    points.Add(new VertexPositionNormal(nextPt.CornerPoints[1], Vector3.Zero));
                    points.Add(new VertexPositionNormal(point.CornerPoints[2], Vector3.Zero));
                    points.Add(new VertexPositionNormal(point.CornerPoints[3], Vector3.Zero));
                    points.Add(new VertexPositionNormal(nextPt.CornerPoints[2], Vector3.Zero));
                    points.Add(new VertexPositionNormal(nextPt.CornerPoints[3], Vector3.Zero));

                    Cube.UpdateVertexData(points.ToArray());

                    GLMaterialBlendState.Translucent.RenderBlendState();

                    Vector4 blockColor = new Vector4(1, 1, 0, 0.2f);
                    Cube.DrawSolid(context, Matrix4.Identity, blockColor);

                    GLMaterialBlendState.Opaque.RenderBlendState();
                }
            }
        }

        public override void DrawLineDisplay(GLContext context, bool picking = false)
        {
            if (InPathMode)
            {
                base.DrawLineDisplay(context, picking);
            }

            List<Vector3> points = new List<Vector3>();
            List<Vector4> colors = new List<Vector4>();

            foreach (CubePathPoint<TPath, TPoint> point in PathPoints)
            {
                if (!point.IsVisible)
                    continue;

                foreach (CubePathPoint<TPath, TPoint> nextPt in point.Children)
                {
                    if (!nextPt.IsVisible)
                        continue;

                    for (int i = 0; i < 4; i++)
                    {
                        points.Add(point.CornerPoints[i]);
                        points.Add(nextPt.CornerPoints[i]);

                        //Color by side
                        if (i % 2 == 0)
                        {
                            colors.Add(new Vector4(0, 0.8f, 0, 1));
                            colors.Add(new Vector4(0, 1, 0, 1));
                        }
                        else
                        {
                            colors.Add(new Vector4(0.8f, 0, 0, 1));
                            colors.Add(new Vector4(1, 0, 0, 1));
                        }

                        //Scale the arrow with the current point size
                        Vector3 scale = new Vector3(3);
                        scale *= PointSize;
                        scale *= point.CameraScale;

                        if (InPathMode)
                            scale *= 0.2f;

                        //Get the distance between the next and current point
                        Vector3 dist = nextPt.CornerPoints[i] - point.CornerPoints[i];
                        //Rotate based on the direction of the distance to point at the next point
                        var rotation = RotationFromTo(new Vector3(0, 0.0000001f, 1), dist.Normalized());


                        //Keep the cone rotated 90 degrees to not face upwards
                        var rot = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90));
                        //Offset the arrow slightly from it's position
                        Matrix4 offsetMat = Matrix4.CreateTranslation(new Vector3(0, 0, -10));
                        Matrix4 translateMat = Matrix4.CreateTranslation(nextPt.CornerPoints[i]);

                        if (IsArrowCentered)
                        {
                            offsetMat = Matrix4.Identity;
                            //Use the center point between the distance
                            translateMat = Matrix4.CreateTranslation((point.CornerPoints[i] + (dist / 2f)));
                        }

                        //Load the cone render
                        if (ConeRenderer == null)
                            ConeRenderer = new ConeRenderer(10, 2, 15, 16);

                        //Draw the cone with a solid shader
                        Matrix4 modelMatrix = offsetMat * Matrix4.CreateScale(scale) * rotation * translateMat;
                        if (i % 2 == 0)
                            ConeRenderer.DrawSolid(context, rot * modelMatrix, new Vector4(0, 1, 0, 1));
                        else
                            ConeRenderer.DrawSolid(context, rot * modelMatrix, new Vector4(1, 0, 0, 1));
                    }
                }
            }

            if (LineRenderer == null)
                LineRenderer = new LineRender();

            LineMaterial.hasVertexColors = true;
            LineMaterial.Color = Vector4.One;
            LineMaterial.Render(context);

            GL.LineWidth(LineWidth);
            LineRenderer.Draw(points, colors, true);

            GL.LineWidth(1.0F);
        }

        static Matrix4 RotationFromTo(Vector3 start, Vector3 end)
        {
            var axis = Vector3.Cross(start, end).Normalized();

            var angle = Vector3.CalculateAngle(start, end);
            return Matrix4.CreateFromAxisAngle(axis, angle);
        }

        /// <summary>
        /// Generates a list of paths usable for byaml conversion from the current renderer.
        /// </summary>
        /// <returns></returns>
        public override List<PathBase<TPath, TPoint>> GeneratePaths()
        {
            List<PathBase<TPath, TPoint>> groups = new List<PathBase<TPath, TPoint>>();

            //First go through all paths and get all the groups
            for (int i = 0; i < PathPoints.Count; i++)
            {
                var point = PathPoints[i] as CubePathPoint<TPath, TPoint>;
                var pt = point.Tag;
                pt.Translate = new ByamlVector3F(
                    point.Transform.Position.X,
                    point.Transform.Position.Y,
                    point.Transform.Position.Z);
                pt.Scale = new ByamlVector3F(
                    point.Transform.Scale.X,
                    point.Transform.Scale.Y,
                    point.Transform.Scale.Z);
                pt.Rotate = new ByamlVector3F(
                    point.Transform.RotationEuler.X,
                    point.Transform.RotationEuler.Y,
                    point.Transform.RotationEuler.Z);

                if (!groups.Contains(point.Group))
                {
                    //Reset the path list
                    point.Group.Points = new List<TPoint>();
                    //Reset return point list
                    if (point.Group is LapPath)
                        (point.Group as LapPath).ReturnPoints = new List<ReturnPoint>();

                    //Add the group instance
                    groups.Add(point.Group);
                }

                //Add the group's point
                point.Group.Points.Add(point.Tag);
                //Add the return point
                if (point.Group is LapPath) {
                    //Local to world space
                    var worldPos = point.Transform.Position + point.ReturnPoint.Transform.Position;
                    var localRotation = point.ReturnPoint.Transform.Rotation;

                    //Row 0 binormal Row1 normal Row2 tangent
                    Matrix4 rotationMat = Matrix4.CreateFromQuaternion(localRotation);
                    Vector3 nrm = new Vector3(rotationMat.Row1);
                    Vector3 tan = new Vector3(rotationMat.Row2);

                    point.ReturnPoint.Tag.Position = new ByamlVector3F(
                        worldPos.X, worldPos.Y, worldPos.Z);
                    point.ReturnPoint.Tag.Tangent = new ByamlVector3F(
                        tan.X, tan.Y, tan.Z);
                    point.ReturnPoint.Tag.Normal = new ByamlVector3F(
                        nrm.X, nrm.Y, nrm.Z);

                    (point.Group as LapPath).ReturnPoints.Add(point.ReturnPoint.Tag);

                    point.LapPoint.ReturnPoint = point.ReturnPoint.Tag;
                }

                //Add the children's points
                point.Tag.NextPoints = new List<TPoint>();
                foreach (CubePathPoint<TPath, TPoint> child in point.Children)
                    point.Tag.NextPoints.Add(child.Tag);

                //Add the parent points
                point.Tag.PreviousPoints = new List<TPoint>();
                foreach (CubePathPoint<TPath, TPoint> child in point.Parents)
                    point.Tag.PreviousPoints.Add(child.Tag);

            }
            return groups;
        }

        public override RenderablePathPoint GetHoveredPoint()
        {
            var context = GLContext.ActiveContext;
            //hovered points
            return context.ColorPicker.FindPickableAtPosition(
                context, this.PathPoints.Where(x => !x.IsSelected).Cast<ITransformableObject>().ToList(),
                new Vector2(context.CurrentMousePoint.X, context.Height - context.CurrentMousePoint.Y)) as RenderablePathPoint;

            /*      new Vector2(context.CurrentMousePoint.X, context.Height - context.CurrentMousePoint.Y)) as RenderablePathPoint;
             return context.RayPicker.FindPickableAtPosition(
                  context, this.PathPoints.Where(x => !x.IsSelected).Cast<IRayCastPicking>().ToList(),
                  new Vector2(context.CurrentMousePoint.X, context.Height - context.CurrentMousePoint.Y)) as RenderablePathPoint;*/
        }

        public override void Dispose()
        {
            this.ConeRenderer?.Dispose();

            base.Dispose();
        }
    }
}
