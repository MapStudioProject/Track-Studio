using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using MapStudio.UI;
using ImGuiNET;
using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace TurboLibrary
{
    public class MinimapTextureGen
    {
        private GLTexture2D ModelNormalsTexture;
        private GLTexture2D FilteredTexture;

        public Vector3 Color = new Vector3(1.0f);

        public void Update(Workspace workspace)
        {
            var shading = DebugShaderRender.DebugRendering;

            DebugShaderRender.DebugRendering = DebugShaderRender.DebugRender.Normal;

            ModelNormalsTexture?.Dispose();
            ModelNormalsTexture = workspace.ViewportWindow.SaveAsScreenshotGLTexture(1024, 1024);

            FilterMinimap();

            DebugShaderRender.DebugRendering = shading;
        }

        public void FilterMinimap()
        {
            if (ModelNormalsTexture == null)
                return;

            if (FilteredTexture == null)
                FilteredTexture = GLTexture2D.CreateUncompressedTexture(448, 448, PixelInternalFormat.Rgba);

            Framebuffer fbo = new Framebuffer(FramebufferTarget.Framebuffer);
            fbo.Bind();

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, FilteredTexture.ID, 0);

            GL.Viewport(0, 0, FilteredTexture.Width, FilteredTexture.Height);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            var shader = GlobalShaders.GetShader("MINIMAP");
            shader.Enable();

            if (fbo.Width != FilteredTexture.Width || fbo.Height != FilteredTexture.Height)
                fbo.Resize(FilteredTexture.Width, FilteredTexture.Height);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.SetTexture(ModelNormalsTexture, "normalsTexture", 1);
            shader.SetVector3("color", new OpenTK.Vector3(Color.X, Color.Y, Color.Z));

            ScreenQuadRender.Draw();

            GL.Flush();

            fbo.Unbind();
            fbo.Dispose();

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.UseProgram(0);
            GL.Viewport(0, 0, GLContext.ActiveContext.Width, GLContext.ActiveContext.Height);

       //     FilterImageSharp();
        }

        public void Draw()
        {
            if (ModelNormalsTexture == null)
                return;

            ImGui.Image((IntPtr)FilteredTexture.ID, new Vector2(448, 488));
        }

        private void FilterImageSharp()
        {
            byte[] data = FilteredTexture.GetBytes();
            byte[] newData = ImageSharpTextureHelper.DropShadows(data, FilteredTexture.Width, FilteredTexture.Height);
            FilteredTexture.Reload(FilteredTexture.Width, FilteredTexture.Height, newData);
        }
    }
}
