using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using AGraphicsLibrary;
using Toolbox.Core;
using GLFrameworkEngine;

namespace AGraphicsLibrary
{
    public class LightmapManager
    {
        public static Framebuffer FilterLevel0;
        public static Framebuffer FilterLevel1;

        public static GLTexture2D NormalsTexture;

        public static Vector2 Offset { get; set; }

        public static void Init(int size)
        {
            DrawBuffersEnum[] buffers = new DrawBuffersEnum[6]
            {
                DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3,
                DrawBuffersEnum.ColorAttachment4, DrawBuffersEnum.ColorAttachment5,
           };

            FilterLevel0 = new Framebuffer(FramebufferTarget.Framebuffer);
            FilterLevel1 = new Framebuffer(FramebufferTarget.Framebuffer);
            FilterLevel0.SetDrawBuffers(buffers);
            FilterLevel1.SetDrawBuffers(buffers);

            var normalsDDS = new DDS(new System.IO.MemoryStream(CafeLibrary.Properties.Resources.normals));

            NormalsTexture = GLTexture2D.FromGeneric(normalsDDS, new ImageParameters());

            NormalsTexture.Bind();
            NormalsTexture.WrapR = TextureWrapMode.ClampToEdge;
            NormalsTexture.WrapT = TextureWrapMode.ClampToEdge;
            NormalsTexture.WrapS = TextureWrapMode.ClampToEdge;
            NormalsTexture.MinFilter = TextureMinFilter.Nearest;
            NormalsTexture.MagFilter = TextureMagFilter.Nearest;
            NormalsTexture.UpdateParameters();
            NormalsTexture.Unbind();
        }

        public static void CreateLightmapTexture(GLContext control, AglLightMap aglLightMap,
            EnvironmentGraphics environmentSettings, string name, GLTexture output)
        {
            var lightMapEnv = aglLightMap.LightAreas.FirstOrDefault(x => x.Settings.Name == name);
            if (lightMapEnv == null)
                return;

            bool is2DArray = output is GLTexture2DArray;

            //Force generate mipmaps to update the mip allocation so mips can be assigned.
            output.Bind();
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.Enable(EnableCap.TextureCubeMapSeamless);

            if (FilterLevel0 == null)
                Init(output.Width);

            int CUBE_SIZE = output.Width;

            FilterLevel0.Bind();
            LoadCubemapLevel(control, CUBE_SIZE, 0, aglLightMap, environmentSettings, lightMapEnv, output.ID, is2DArray);
            FilterLevel0.Unbind();

            FilterLevel1.Bind();
            LoadCubemapLevel(control, CUBE_SIZE / 2, 1, aglLightMap, environmentSettings, lightMapEnv, output.ID, is2DArray);
            FilterLevel1.Unbind();

            control.UpdateViewport = true;
        }

        static void LoadCubemapLevel(GLContext control, int size, int level, AglLightMap aglLightMap, 
            EnvironmentGraphics environmentSettings, AglLightMap.LightArea lightMapEnv, int ID, bool is2DArray)
        {
            GL.Viewport(0, 0, size, size);

            var shader = GlobalShaders.GetShader("LIGHTMAP");
            shader.Enable();

            UpdateUniforms(control, shader, level, aglLightMap, environmentSettings, lightMapEnv);

            for (int i = 0; i < 6; i++)
            {
                if (is2DArray)
                {
                    GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer,
                        FramebufferAttachment.ColorAttachment0 + i, ID, level, i);
                }
                else
                {
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
               FramebufferAttachment.ColorAttachment0 + i,
               TextureTarget.TextureCubeMapPositiveX + i, ID, level);
                }
            }

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            ScreenQuadRender.Draw();

            var errorcheck = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (errorcheck != FramebufferErrorCode.FramebufferComplete)
                throw new Exception(errorcheck.ToString());

            GL.UseProgram(0);
        }

        static void UpdateUniforms(GLContext control, ShaderProgram shader, int mipLevel,
           AglLightMap aglLightMap,  EnvironmentGraphics environmentSettings, AglLightMap.LightArea lightMapEnv)
        {
            if (aglLightMap.TextureLUT == null)
                aglLightMap.Setup();

            GL.ActiveTexture(TextureUnit.Texture0 + 1);
            NormalsTexture.Bind();
            shader.SetInt("uNormalTex", 1);

            GL.ActiveTexture(TextureUnit.Texture0 + 2);
            aglLightMap.TextureLUT.Bind();
            shader.SetInt("uLutTex", 2);

            var settings = lightMapEnv.Settings;

            shader.SetFloat($"settings.rim_angle", settings.rim_angle);
            shader.SetFloat($"settings.rim_width", settings.rim_width);
            shader.SetInt($"settings.type", settings.lighting_hint);

            for (int i = 0; i < 6; i++)
            {
                shader.SetVector3($"lights[{i}].dir", new Vector3(0));
                shader.SetVector4($"lights[{i}].lowerColor", new Vector4(0));
                shader.SetVector4($"lights[{i}].upperColor", new Vector4(0));
                shader.SetInt($"lights[{i}].lutIndex", 0);
            }

            int index = 0;
            //Loop through the light sources and apply them from the env settings
            foreach (var light in lightMapEnv.Lights)
            {
                //If the name is empty, skip as there is no way to find it.
                if (string.IsNullOrEmpty(light.Name))
                    continue;

                //Skip mip levels for specific light sources
                if (!light.enable_mip0 && mipLevel == 0 || !light.enable_mip1 && mipLevel == 1)
                    continue;

                LightSource lightSource = null;
                switch (light.Type)
                {
                    case "DirectionalLight":
                        var dir = environmentSettings.DirectionalLights.FirstOrDefault(x => x.Name ==  light.Name);
                        if (dir != null && dir.Enable)
                            lightSource = LoadDirectionalLighting(dir);
                        break;
                    case "HemisphereLight":
                        var hemi = environmentSettings.HemisphereLights.FirstOrDefault(x => x.Name == light.Name);
                        if (hemi != null && hemi.Enable)
                            lightSource = LoadHemiLighting(hemi);
                        break;
                    case "AmbientLight":
                        var ambient = environmentSettings.AmbientLights.FirstOrDefault(x => x.Name == light.Name);
                        if (ambient != null && ambient.Enable)
                            lightSource = LoadAmbientLighting(ambient);
                        break;
                }

                if (lightSource == null)
                    continue;

                //Setup shared settings
                lightSource.LutIndex = aglLightMap.GetLUTIndex(light.LutName);

                int programID = shader.program;

                shader.SetVector3($"lights[{index}].dir", lightSource.Direction);
                shader.SetVector4($"lights[{index}].lowerColor", lightSource.LowerColor);
                shader.SetVector4($"lights[{index}].upperColor", lightSource.UpperColor);
                shader.SetInt($"lights[{index}].lutIndex", lightSource.LutIndex);
                index++;
            }
            lightMapEnv.Initialized = true;
        }

        static LightSource LoadDirectionalLighting(DirectionalLight dirLight)
        {
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(dirLight.Direction.X, dirLight.Direction.Y, dirLight.Direction.Z);
            lightSource.LowerColor = dirLight.BacksideColor.ToVector4() * dirLight.Intensity;
            lightSource.UpperColor = dirLight.DiffuseColor.ToVector4() * dirLight.Intensity;
            return lightSource;
        }

        static LightSource LoadHemiLighting(HemisphereLight hemiLight)
        {
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(0, -1, 0);
            lightSource.LowerColor = hemiLight.GroundColor.ToVector4() * hemiLight.Intensity;
            lightSource.UpperColor = hemiLight.SkyColor.ToVector4() * hemiLight.Intensity;
            return lightSource;
        }

        static LightSource LoadAmbientLighting(AmbientLight ambLight)
        {
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(0, -1, 0);
            lightSource.LowerColor = ambLight.Color.ToVector4() * ambLight.Intensity;
            lightSource.UpperColor = ambLight.Color.ToVector4() * ambLight.Intensity;
            return lightSource;
        }

        class LightSource
        {
            public Vector3 Direction = new Vector3(0, 0, 0);

            public Vector4 LowerColor = new Vector4(0, 0, 0, 1);
            public Vector4 UpperColor = new Vector4(0, 0, 0, 1);

            //The index for the LUT texture (determines the y texture coordinate)
            public int LutIndex;
        }
    }
}
