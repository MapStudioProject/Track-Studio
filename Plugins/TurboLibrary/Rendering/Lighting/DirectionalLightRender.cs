using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using AGraphicsLibrary;

namespace TurboLibrary.Rendering
{
    public class DirectionalLightRender : ITransformableObject, IRayCastPicking, IDrawable
    {
        public bool IsVisible { get; set; } = true;

        public GLTransform Transform { get; set; } = new GLTransform();

        public bool IsHovered { get; set; }
        public bool IsSelected { get; set; }
        public bool CanSelect { get; set; } = true;

        PlaneRenderer PlaneRenderer = null;
        LineRender LineRender = null;
        GLTexture2D SunTexture;

        float CameraScale = 1.0f;

        public BoundingNode GetRayBounding()
        {
            return new BoundingNode()
            {
                Radius = 15 * CameraScale,
            };
        }

        public DirectionalLightRender()
        {
            SunTexture = GLTexture2D.FromBitmap(Properties.Resources.Sun);

            var env = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];
            var direction = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"].DirectionalLights.FirstOrDefault();

            Transform.Position = new Vector3(0, 100, 0);
            Transform.TransformUpdated += delegate
            {
                var normal = Vector3.TransformNormal(new Vector3(0, 1, 0), Matrix4.CreateFromQuaternion(Transform.Rotation));
                direction.Direction = new Syroot.Maths.Vector3F(normal.X, normal.Y, normal.Z);

                foreach (var lightmap in LightingEngine.LightSettings.Resources.LightMapFiles.FirstOrDefault().Value.Lightmaps)
                    LightingEngine.LightSettings.UpdateLightmap(GLContext.ActiveContext, lightmap.Value.Name);
            };
        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE)
                return;

            if (PlaneRenderer == null) {
                PlaneRenderer = new PlaneRenderer(5);
                LineRender = new LineRender();
            }

            UpdateTransform();

            CameraScale = context.Camera.ScaleByCameraDistance(Transform.Position);

            var scaleMatrix = Matrix4.CreateScale(CameraScale);
            var transform = scaleMatrix * Transform.TransformMatrix;

            PlaneRenderer.DrawBillboardSprite(context, SunTexture.ID, transform);
            DrawDashedLine(context, transform);
        }

        void DrawDashedLine(GLContext context, Matrix4 transform)
        {
            context.CurrentShader = GlobalShaders.GetShader("LINE_DASHED");
            context.CurrentShader.SetVector4("color", new Vector4(0.5F, 0.5F, 0.5F, 1));
            context.CurrentShader.SetMatrix4x4(GLConstants.ModelMatrix, ref transform);

            context.CurrentShader.SetVector2("viewport_size", new Vector2(context.Width, context.Height));
            context.CurrentShader.SetFloat("dash_factor", 5);
            context.CurrentShader.SetFloat("dash_width", 5);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.5F);

            GL.LineWidth(2.0f);
            LineRender.Draw(new Vector3(0), new Vector3(0, 200, 0), new Vector4(1), false);
            GL.LineWidth(1.0f);

            GL.Disable(EnableCap.AlphaTest);
        }

        public void Dispose()
        {
            PlaneRenderer?.Dispose();
            LineRender?.Dispose();
        }

        private void UpdateTransform()
        {
            var direction = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"].DirectionalLights.FirstOrDefault();
            var quat = RotateFromNormal(new Vector3(
                direction.Direction.X, direction.Direction.Y, direction.Direction.Z),
                new Vector3(0, 1, 0));

            if (Transform.Rotation != quat)
            {
                Transform.Rotation = quat;
                Transform.UpdateMatrix(true);
            }
        }

        static Quaternion RotateFromNormal(Vector3 normal, Vector3 up)
        {
            if (normal == up)
                return Quaternion.Identity;

            var axis = Vector3.Normalize(Vector3.Cross(up, normal));
            float angle = MathF.Acos(Vector3.Dot(up, normal));

            if (!float.IsNaN(axis.X) && !float.IsNaN(axis.Y) && !float.IsNaN(axis.Z) && !float.IsNaN(angle))
                return Quaternion.FromAxisAngle(axis, angle);

            return Quaternion.Identity;
        }
    }
}
