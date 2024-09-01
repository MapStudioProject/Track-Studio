using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Toolbox.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using GLFrameworkEngine;

namespace AGraphicsLibrary
{
    public class CubemapHDREncodeRT
    {
        public static void CreateCubemap(GLTextureCube cubemapInput, GLTexture cubemapOutput,
            int layer, int numMips, bool srgbToLinear = true, bool flipImage = false)
        {
            int size = cubemapOutput.Width;

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1.0f, 0.1f, 10.0f);
            Matrix4[] captureViews = {
                 Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                 Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                 Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                 Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)),
                 Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                 Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)),
            };

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            //Bind the cubemap's texture into a filtered quad. 
            //Bind the drawn filter to a cubemap array layer
            Framebuffer frameBuffer = new Framebuffer(FramebufferTarget.Framebuffer, size, size, PixelInternalFormat.Rgba32f);
            frameBuffer.Bind();

            GL.Disable(EnableCap.Blend);

            if (numMips > 0)
            {
                cubemapOutput.Bind();
                cubemapOutput.GenerateMipmaps();
                cubemapOutput.Unbind();
            }

            var cubemapFilter = GlobalShaders.GetShader("CUBEMAP_HDRENCODE");
            cubemapFilter.Enable();

            GL.ActiveTexture(TextureUnit.Texture0 + 1);
            cubemapInput.Bind();
            cubemapFilter.SetInt("cubemapTexture", 1);

            cubemapFilter.SetMatrix4x4("projection", ref projection);
            cubemapFilter.SetFloat("range", 1024.0f);
            cubemapFilter.SetFloat("scale", 4.0f);
            cubemapFilter.SetBoolToInt("srgbToLinear", srgbToLinear);
            cubemapFilter.SetBoolToInt("flipY", flipImage);

            //Quick hack, draw once before rendering (first buffer not updating for some reason??)
            RenderTools.DrawCube();

            GL.Disable(EnableCap.CullFace);
            for (int mip = 0; mip < numMips; mip++)
            {
                int mipWidth = (int)(size * Math.Pow(0.5, mip));
                int mipHeight = (int)(size * Math.Pow(0.5, mip));

                frameBuffer.Resize(mipWidth, mipHeight);
                GL.Viewport(0, 0, mipWidth, mipHeight);

                cubemapFilter.SetFloat("mipLevel", mip);
                for (int i = 0; i < 6; i++)
                {
                    cubemapFilter.SetInt("faceLevel", i);

                    //attach face to fbo as color attachment 0
                    if (cubemapOutput is GLTexture2DArray)
                    {
                        GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer,
                             FramebufferAttachment.ColorAttachment0, cubemapOutput.ID, mip, (layer * 6) + i);
                    }
                    else if (cubemapOutput is GLTextureCubeArray)
                    {
                        GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer,
                             FramebufferAttachment.ColorAttachment0, cubemapOutput.ID, mip, (layer * 6) + i);
                    }
                    else
                    {
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                                 FramebufferAttachment.ColorAttachment0,
                                  TextureTarget.TextureCubeMapPositiveX + i, cubemapOutput.ID, mip);
                    }

                    cubemapFilter.SetMatrix4x4("view", ref captureViews[i]);

                    GL.ClearColor(0, 0, 0, 1);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    RenderTools.DrawCube();
                }
            }

            var errorcheck = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (errorcheck != FramebufferErrorCode.FramebufferComplete)
                throw new Exception(errorcheck.ToString());

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);

            frameBuffer.Dispose();
            frameBuffer.DisposeRenderBuffer();

            GL.UseProgram(0);
        }
    }
}
