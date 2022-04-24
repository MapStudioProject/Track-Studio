using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using GLFrameworkEngine;

namespace AGraphicsLibrary
{
    public class CausticLightManager
    {
        public static void DrawCaustics(GLContext context, GLTexture gbuffer, GLTexture linearDepth)
        {
            return;

            foreach (var obj in context.Scene.Objects) {
                if (obj is GenericRenderer)
                    ((GenericRenderer)obj).DrawCaustics(context, gbuffer, linearDepth);
            }
        }

        public static void PrepareMaterial(GLContext context, GLTexture indTexture, 
            GLTexture patternTexture,  GLTexture gbuffer, GLTexture linearDepth)
        {
            var shader = GlobalShaders.GetShader("LPP_CAUSTICS");
            context.CurrentShader = shader;

            Vector3 lightDir = context.Scene.ShadowRenderer.GetLightDirection();

            Matrix4 lightSpaceMatrix = Matrix4.LookAt(
                new Vector3(0, 0, 0.1f), 
                new Vector3(0), 
                new Vector3(0, 1, 0));
            Matrix4 lightProj = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, 0.01f, 5.0f);
            Matrix4 lightVP = lightProj * lightSpaceMatrix;

          /*  lightVP.Column0 = new Vector4(-0.2408499f, 0.5407615f, 0.8059581f, 0);
            lightVP.Column1 = new Vector4(1);
            lightVP.Column2 = new Vector4(1);
            lightVP.Column3 = new Vector4(-6000, 490, 0.2f, 0.2f);*/

            Vector2 projOffset = new Vector2(0);
            Vector2 projScale = new Vector2(1.0f);

            shader.SetTexture(indTexture, "IndirectTexture", 1);
            shader.SetTexture(patternTexture, "PatternTexture", 2);
            shader.SetTexture(gbuffer, "NormalsTexture", 3);
            shader.SetTexture(linearDepth, "LinearDepthTexture", 4);
            shader.SetVector4("viewParams", new OpenTK.Vector4(context.Height, context.Width, 0, 0));

            var viewMat = context.Camera.ViewMatrix;
            var projMat = context.Camera.ProjectionMatrix;
            var viewProjMatInv = context.Camera.ViewProjectionMatrix.Inverted();

            shader.SetMatrix4x4("mtxView", ref viewMat);
            shader.SetMatrix4x4("mtxProj", ref projMat);
            shader.SetMatrix4x4("mtxViewProjInv", ref viewProjMatInv);
            shader.SetMatrix4x4("mtxLightVP", ref lightVP);

            shader.SetFloat("clipRange", context.Camera.ZFar - context.Camera.ZNear);
            shader.SetFloat("clipDiv", context.Camera.ZNear / context.Camera.ZFar);
            shader.SetFloat("clipNear", context.Camera.ZNear);
            shader.SetFloat("clipFar", context.Camera.ZFar);
            shader.SetVector2("viewAspect", new OpenTK.Vector2(
                context.Camera.FactorX, context.Camera.FactorY));

            shader.SetVector2("viewSize", new OpenTK.Vector2(context.Width, context.Height));

            shader.SetVector3("cameraPos", context.Camera.GetViewPostion());
            shader.SetVector3("cameraDir", context.Camera.InverseRotationMatrix.Row2);

            shader.SetVector2("projectionOffset", projOffset);
            shader.SetVector2("projectionScale", projScale);
            shader.SetFloat("clipRange", context.Camera.ZFar - context.Camera.ZNear);
            shader.SetFloat("fov_x", context.Camera.Fov);
            shader.SetFloat("fov_y", context.Camera.Fov);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }
    }
}
