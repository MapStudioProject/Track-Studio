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
    public class CubePathPoint<TPath, TPoint> : PathPoint<TPath, TPoint>, IColorPickable
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        public LapPathPoint LapPoint => Tag as LapPathPoint;
        public GravityPathPoint GravityPathPoint => Tag as GravityPathPoint;

        public ReturnPointPickable ReturnPoint { get; set; }

        public override BoundingNode GetRayBounding()
        {
            return null;
        }

        public Vector3[] CornerPoints = new Vector3[4];

        class CubePointRender : RenderMesh<VertexPositionNormalColor>
        {
            public CubePointRender(PrimitiveType primitiveType = PrimitiveType.Triangles) :
                base(GetCubeVertices2(1.0f), Indices, primitiveType)
            {

            }

            public static VertexPositionNormalColor[] GetCubeVertices2(float size)
            {
                Vector3 topColor = new Vector3(1, 0, 0);

                VertexPositionNormalColor[] vertices = new VertexPositionNormalColor[8];
                vertices[0] = new VertexPositionNormalColor(new Vector3(-1, -1, 1), new Vector3(-1, -1, 1), Vector3.One); //Bottom Left
                vertices[1] = new VertexPositionNormalColor(new Vector3(1, -1, 1), new Vector3(1, -1, 1), Vector3.One); //Bottom Right
                vertices[2] = new VertexPositionNormalColor(new Vector3(1, 1, 1), new Vector3(1, 1, 1), topColor); //Top Right
                vertices[3] = new VertexPositionNormalColor(new Vector3(-1, 1, 1), new Vector3(-1, 1, 1), topColor); //Top Left
                vertices[4] = new VertexPositionNormalColor(new Vector3(-1, -1, -1), new Vector3(-1, -1, -1), Vector3.One); //Bottom Left -Z
                vertices[5] = new VertexPositionNormalColor(new Vector3(1, -1, -1), new Vector3(1, -1, -1), Vector3.One); //Bottom Right -Z
                vertices[6] = new VertexPositionNormalColor(new Vector3(1, 1, -1), new Vector3(1, 1, -1), topColor); //Top Right -Z
                vertices[7] = new VertexPositionNormalColor(new Vector3(-1, 1, -1), new Vector3(-1, 1, -1), topColor); //Top Left -Z
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i].Position *= size;
                return vertices;
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
                    0, 1, 5, 5, 4, 0,
                    // right face
                    1, 5, 6, 6, 2, 1,
            };
        }

        CubePointRender CubeRender;

        public CubePathPoint(RenderablePath path, Vector3 position)
                 : base(path, position)
        {
            CreateNewObject();
            UINode.GetHeader += delegate
            {
                string header = this.Name;
                if (typeof(TPath) == typeof(LapPath))
                {
                    if (LapPoint.LapCheck != -1)
                        header += $" Lap#{LapPoint.LapCheck}";
                    if (LapPoint.CheckPoint != -1)
                        header += $" CheckPt#{LapPoint.CheckPoint}";
                }
                return header;
            };
            ReloadIcons();

            if (typeof(TPath) == typeof(LapPath))
            {
                LapPoint.PropertyChanged += delegate {
                    ReloadIcons();
                };
                UINode.TagUI.UIDrawer += delegate
                {
                    var lapPath = LapPoint.Path;
                    LapPathUI.Render(lapPath);
                };
            }
            if (typeof(TPath) == typeof(GravityPath))
            {
                UINode.TagUI.UIDrawer += delegate
                {
                    var gravityPath = GravityPathPoint.Path;
                    GravityPathUI.Render(gravityPath);
                };
            }
        }

        private void ReloadIcons()
        {
            UINode.Icon = ParentPath.UINode.Icon;
            UINode.IconColor = ParentPath.UINode.IconColor;
        }

        public override RenderablePathPoint Duplicate()
        {
            var point = (CubePathPoint<TPath, TPoint>)ParentPath.CreatePoint(Transform.Position);

            if (this.Tag is LapPathPoint)
            {
                var srcTag = this.Tag as LapPathPoint;
                point.Tag = ((LapPathPoint)srcTag).Clone() as TPoint;
            }
            return point;
        }

        public void DrawColorPicking(GLContext context)
        {
            if (!IsVisible)
                return;

            var matrix = Transform.TransformNoScaleMatrix;
            matrix = Matrix4.CreateScale(Transform.Scale.X / 2f, Transform.Scale.Y / 2f, 10.0f) * matrix;

            if (CubeRender == null)
                CubeRender = new CubePointRender();

            CubeRender.UpdatePrimitiveType(PrimitiveType.Triangles);
            CubeRender.DrawPicking(context, this, matrix);
        }

        public override void Render(GLContext context, Pass pass)
        {
            if (CubeRender == null)
                CubeRender = new CubePointRender();

            if (pass == Pass.TRANSPARENT)
            {
                var matrix = Transform.TransformNoScaleMatrix;
                matrix = Matrix4.CreateScale(Transform.Scale.X / 2f, Transform.Scale.Y / 2f, 10.0f) * matrix;

                GLMaterialBlendState.Translucent.RenderBlendState();

                Vector4 color = ParentPath.PointColor;
                if (LapPoint != null && LapPoint.CheckPoint != -1)
                    color = new Vector4(0, 0.5f, 0.9f, color.W);
                if (LapPoint != null && LapPoint.LapCheck != -1)
                    color = new Vector4(0, 1, 0, color.W);

                CubeRender.UpdatePrimitiveType(PrimitiveType.Triangles);

                var standard = new StandardMaterial();
                standard.Color = color;

                //standard.hasVertexColors = true;
                standard.ModelMatrix = matrix;
                standard.DisplaySelection = IsSelected || IsHovered;
                standard.Render(context);
                CubeRender.DrawWithSelection(context, standard.DisplaySelection);

                GL.LineWidth(1);
                CubeRender.UpdatePrimitiveType(PrimitiveType.Lines);
                CubeRender.DrawSolid(context, matrix, Vector4.One);

                GLMaterialBlendState.Opaque.RenderBlendState();
            }
            if (pass == Pass.OPAQUE)
            {
                if (ReturnPoint != null)
                    ReturnPoint.Draw(context);
            }
        }


        private void CreateNewObject()
        {
          //  Transform.Config.ForceLocalScale = true;
            Transform.TransformUpdated += delegate {
                CornerPoints = GetCornerPoints();
            };
            Transform.CustomScaleActionCallback += (sender, e) =>
            {
                //Need to make the scale ignore the Z value for cube path types.
                //While they can get used in a rare case, they should be edited manually by UI instead to avoid constant changing.
               var arguments = sender as GLTransform.CustomScaleArgs;

                var settings = GLContext.ActiveContext.TransformTools.TransformSettings;

                if (settings.PivotMode != TransformSettings.PivotSpace.Individual && !Transform.Config.ForceLocalScale)
                    Transform.Position = (arguments.PreviousTransform.Position - arguments.Origin) * arguments.Scale + arguments.Origin;

                Transform.Scale = new Vector3(
                    arguments.PreviousTransform.Scale.X * new Vector3(arguments.Rotation.Row0 * arguments.Scale).Length,
                    arguments.PreviousTransform.Scale.Y * new Vector3(arguments.Rotation.Row1 * arguments.Scale).Length,
                    arguments.PreviousTransform.Scale.Z
                    );
                Transform.UpdateMatrix(true);
            };

            //Create a new point and assign a new group to it
            //If the point is connected to a child, its group will be used instead
            //If a point is by itself, it'll use its own group
            //If a point joins a new group, it'll still use its own group
            Tag = (TPoint)Activator.CreateInstance(PointType);
            Transform.Scale = new Vector3(1000, 600, 25) * GLContext.PreviewScale;
            //Transform.Rotation = Quaternion.FromEulerAngles(0, -GLContext.ActiveContext.Camera.RotationY, 0);

            //Attach return points to newly made lap points
            if (typeof(TPath) == typeof(LapPath))
            {
                ReturnPoint = new ReturnPointPickable(this);
            }

            Transform.UpdateMatrix(true);
        }

        private Vector3[] GetCornerPoints()
        {
            Vector3[] cube = new Vector3[4]
               {
                        new Vector3(-1,-1, 1),
                        new Vector3(1,-1, 1),
                        new Vector3(-1, 1, 1),
                        new Vector3(1, 1, 1),
               };

            Vector3 scale = new Vector3(Transform.Scale.X, Transform.Scale.Y, 0.0f) / 2;
            Matrix4 pointTransform = Matrix4.CreateScale(scale) * Transform.TransformNoScaleMatrix;
            for (int i = 0; i < 4; i++)
                cube[i] = Vector3.TransformPosition(cube[i], pointTransform);

            return cube;
        }

        public override void OnGroupUpdated()
        {
            if (Group is LapPath)
            {
                //Update return points
                var path = Group as LapPath;
                if (!path.ReturnPoints.Contains(ReturnPoint.Tag))
                    path.ReturnPoints.Add(ReturnPoint.Tag);
            }
            Tag.Path = Group as TPath;
        }
    }

    public class ReturnPointPickable : ITransformableObject, IColorPickable
    {
        public GLTransform Transform { get; set; } = new GLTransform();

        public float CameraScale = 1.0f;

        public bool IsHovered { get; set; }
        public bool IsSelected { get; set; }
        public bool CanSelect
        {
            get { return ParentPoint.CanSelect && !ParentPoint.IsSelected; }
            set { }
        }

        public RenderablePathPoint ParentPoint;

        public ReturnPoint Tag = new ReturnPoint();

        OrientationCubeDrawer CubeRender;
        ConeRenderer ConeRender;
        LineRender LineRender;

        public ReturnPointPickable(RenderablePathPoint point)
        {
            ParentPoint = point;

            var translation = ParentPoint.Transform.Position + Transform.Position;
            Transform.SetCustomOrigin(translation);

            Transform.EnableCollisionDrop = false;
            Transform.IndividualPivot = true;
            Transform.CustomScaleActionCallback += (sender, e) =>
            {
                Transform.UpdateMatrix(true);
            }; 
        }

        public void DrawColorPicking(GLContext context)
        {
            if (CubeRender == null)
                CubeRender = new OrientationCubeDrawer(20);

            //Local to world space, relative from the point it is parented to
            var translation = ParentPoint.Transform.Position + Transform.Position;
            var rotation = ParentPoint.Transform.Rotation * Transform.Rotation;

            var matrix = Matrix4.CreateScale(2) * Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(translation);
            CubeRender.DrawPicking(context, this, matrix);
        }

        public void Draw(GLContext context)
        {
            if (Tag.ReturnType == ReturnPointType.Ignore)
                return;

            if (this.IsSelected)
            {
                var lineMat = new StandardMaterial();
                lineMat.hasVertexColors = true;
                lineMat.Render(context);

                if (LineRender == null)
                    LineRender = new LineRender();

                var trans = ParentPoint.Transform.Position + Transform.Position;

                LineRender.UpdateVertexData(new LineRender.LineVertex[2]
                {
                    new LineRender.LineVertex(ParentPoint.Transform.Position, new Vector4(0, 1, 0, 1)),
                    new LineRender.LineVertex(trans, new Vector4(1, 0, 0, 1)),
                });
                GL.LineWidth(5);
                LineRender.Draw(context);
                GL.LineWidth(1);
            }

            Vector4 color = new Vector4(1, 1, 0, 1);
            if (Tag.ReturnType == ReturnPointType.NoReturnAfterPass)
                color = new Vector4(1, 0.5f, 0, 1);

            if (ConeRender == null)
                ConeRender = new ConeRenderer(40, 2, 60, 4);
            if (CubeRender == null)
                CubeRender = new OrientationCubeDrawer(20);

            //Local to world space, relative from the point it is parented to
            var translation = ParentPoint.Transform.Position + Transform.Position;
            var rotation =  Transform.Rotation;

            Transform.SetCustomOrigin(translation);

            var matrix = Matrix4.CreateScale(2) * Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(translation);
            //Keep the cone rotated 90 degrees to not face upwards
            var rot = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90)) * 
                     Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(45)) * 
                     Matrix4.CreateTranslation(0, 0, 20);
            var arrowMatrix = rot * Matrix4.CreateScale(new Vector3(1.2f, 0.2f, 1)) * matrix;

            var mat = new StandardMaterial();
            mat.Color = color;
            mat.ModelMatrix = matrix;
            mat.HalfLambertShading = true;
            mat.DisplaySelection = IsHovered || IsSelected;
            mat.Render(context);

            CubeRender.DrawModel(context, matrix, IsSelected || IsHovered);

            mat = new StandardMaterial();
            mat.Color = new Vector4(0.95f, 0, 0, 1);
            mat.ModelMatrix = arrowMatrix;
            mat.HalfLambertShading = true;
            mat.DisplaySelection = IsHovered || IsSelected;
            mat.Render(context);
            ConeRender.Draw(context);
        }
    }
}
