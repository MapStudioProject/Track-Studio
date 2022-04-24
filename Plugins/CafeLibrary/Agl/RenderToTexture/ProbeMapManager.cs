using AampLibraryCSharp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLFrameworkEngine;

namespace AGraphicsLibrary
{
    public class ProbeMapManager
    {
        public static ProbeLighting ProbeLighting { get; set; }

        static Dictionary<string, UniformBlock> Blocks = new Dictionary<string, UniformBlock>();

        static float[] LightmapWeight = new float[] { 1.0f, 0.0f };
        static float[] SHWeight = new float[] { 1.0f };
        static int[] LightmapArrayIndex = new int[] {0, 0};

        public static void Prepare(byte[] fileData)
        {
            ProbeLighting = new ProbeLighting();
            ProbeLighting.LoadValues(AampFile.LoadFile(new System.IO.MemoryStream(fileData)));
        }

        public static bool Generate(Vector3 position, out float[] probeData, bool isTriLinear = true)
        {
            probeData = new float[27];
            if (ProbeLighting == null)
                return false;

            return LightProbeMgr.GetInterpolatedSH(ProbeLighting, position, isTriLinear, ref probeData);
        }

        public static ProbeOutput Generate(GLContext control, GLTextureCube diffuseCubemap, int lightmapTexID, Vector3 position)
        {
            if (ProbeLighting == null || diffuseCubemap == null)
                return null;

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            bool isTriLinear = true;

            float[] shData = new float[27];
            bool generated = LightProbeMgr.GetInterpolatedSH(ProbeLighting, position, isTriLinear, ref shData);
            if (!generated) {
                shData = new float[27];
            }

            var data = LightProbeMgr.ConvertSH2RGB(shData);
            FilterProbeVolume(control, LightmapManager.NormalsTexture, diffuseCubemap, data, lightmapTexID, diffuseCubemap.Width, 0);
            FilterProbeVolume(control, LightmapManager.NormalsTexture, diffuseCubemap, data, lightmapTexID, diffuseCubemap.Width / 2, 1);

            control.ScreenBuffer.Bind();
            GL.Viewport(0, 0, control.Width, control.Height);

            return new ProbeOutput()
            {
                ProbeData = shData,
                Generated = generated,
            };
        }

        public class ProbeOutput
        {
            public float[] ProbeData;
            public bool Generated = false;
        }

        static void LoadUniforms(int programID, Vector4[] shData)
        {
            UniformBlock paramBlock = GetBlock("paramBlock");
            paramBlock.Buffer.Clear();
            //Variables from the sharcfb binaries
            paramBlock.Add(new Vector4(LightmapWeight[0], 0, 0, 0));
            paramBlock.Add(new Vector4(LightmapWeight[1], 0, 0, 0));
            paramBlock.Add(new Vector4(LightmapArrayIndex[0], 0, 0, 0));
            paramBlock.Add(new Vector4(LightmapArrayIndex[1], 0, 0, 0));
            paramBlock.Add(new Vector4(SHWeight[0], 0, 0, 0));
            paramBlock.RenderBuffer(programID, "cbuf_block3");

            UniformBlock dataBlock = GetBlock("shBlock");
            dataBlock.Buffer.Clear();            
            dataBlock.Add(shData[0]);
            dataBlock.Add(shData[1]);
            dataBlock.Add(shData[2]);
            dataBlock.Add(shData[3]);
            dataBlock.Add(shData[4]);
            dataBlock.Add(shData[5]);
            dataBlock.Add(shData[6]);
            dataBlock.RenderBuffer(programID, "cbuf_block4");
        }

        static Framebuffer frameBuffer = null;

        static void Init(int size)
        {
            DrawBuffersEnum[] buffers = new DrawBuffersEnum[6]
            {
                DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3,
                DrawBuffersEnum.ColorAttachment4, DrawBuffersEnum.ColorAttachment5,
            };

            frameBuffer = new Framebuffer(FramebufferTarget.Framebuffer, size, size, PixelInternalFormat.R11fG11fB10f, 0, false);
            frameBuffer.SetDrawBuffers(buffers);
        }

        static void FilterProbeVolume(GLContext control, GLTexture2D normalsMap, GLTextureCube diffuseCubemap,
           Vector4[] shData, int lightmapTexID, int size, int mipLevel)
        {
            if (frameBuffer == null)
                Init(size);

            if (frameBuffer.Width != size)
                frameBuffer.Resize(size, size);

            frameBuffer.Bind();
            GL.Viewport(0, 0, size, size);

            //attach face to fbo as color attachment 
            for (int i = 0; i < 6; i++)
            {
                //Each fragment output is a cubemap face
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                    FramebufferAttachment.ColorAttachment0 + i,
                     TextureTarget.TextureCubeMapPositiveX + i, lightmapTexID, mipLevel);
            }

            var shader = GlobalShaders.GetShader("PROBE");
            shader.Enable();

            var programID = shader.program;

            LoadUniforms(programID, shData);

            GL.ActiveTexture(TextureUnit.Texture0 + 1);
            normalsMap.Bind();
            shader.SetInt("sampler0", 1);

            GL.ActiveTexture(TextureUnit.Texture0 + 2);
            diffuseCubemap.Bind();
            shader.SetInt("sampler1", 2);

            //Draw once with 6 fragment outputs to form a cubemap 
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ScreenQuadRender.Draw();

            var errorcheck = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (errorcheck != FramebufferErrorCode.FramebufferComplete)
                throw new Exception(errorcheck.ToString());

            frameBuffer.Unbind();
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.UseProgram(0);
        }

        static UniformBlock GetBlock(string name)
        {
            if (!Blocks.ContainsKey(name))
                Blocks.Add(name, new UniformBlock());

            return Blocks[name];
        }
    }
}
