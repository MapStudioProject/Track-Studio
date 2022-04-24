using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using GLFrameworkEngine;

namespace CafeLibrary.Rendering
{
    public class BloomExtractionTexture
    {
        public static Framebuffer Filter;

        public static void Init()
        {
            Filter = new Framebuffer(FramebufferTarget.Framebuffer,
                640, 720, PixelInternalFormat.Rgba16f, 1);
        }

        public static GLTexture2D FilterScreen(GLContext control, GLTexture2D colorTexture)
        {
            if (Filter == null)
                Init();

            Filter.Bind();
            GL.Viewport(0, 0, control.Width, control.Height);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            var shader = GlobalShaders.GetShader("BLOOM_EXTRACT");
            shader.Enable();
            shader.SetFloat("bloom_intensity", 1.0f);

            if (Filter.Width != control.Width || Filter.Height != control.Height)
                Filter.Resize(control.Width, control.Height);

            GL.ClearColor(System.Drawing.Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ScreenQuadRender.Draw(shader, colorTexture.ID);
            var screenBuffer = (GLTexture2D)Filter.Attachments[0];

            GL.Flush();

            Filter.Unbind();
            GL.BindTexture(TextureTarget.Texture2D, 0);

            control.SetViewportSize();

            return screenBuffer;
        }
    }
}
