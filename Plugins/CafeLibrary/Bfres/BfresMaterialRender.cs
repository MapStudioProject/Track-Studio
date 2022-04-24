using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using BfresLibrary;
using Toolbox.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using AGraphicsLibrary;

namespace CafeLibrary.Rendering
{
    public class BfresMaterialRender : MaterialAsset
    {
        public int AreaIndex { get; set; } = -1;

        public FMAT Material = new FMAT();

        public string LightMap = "";

        public static float Brightness = 1.0f;

        public BfresRender ParentRenderer { get; set; }

        //GL resources
        UniformBlock MaterialBlock;

        public BfresMaterialRender() { }

        public BfresMaterialRender(BfresRender render, BfresModelRender model) {
            ParentRenderer = render;

            MaterialBlock = new UniformBlock();
        }

        /// <summary>
        /// Gets the param area from a bgenv lighting file.
        /// The engine uses a collect model which as boundings for areas to set various things like fog or lights.
        /// This renderer uses it to determine what fog to render and cubemap (if a map object that dynamically changes areas)
        /// </summary>
        /// <returns></returns>
        public int GetAreaIndex(Vector3 position)
        {
            if (Material.ShaderParams.ContainsKey("gsys_area_env_index_diffuse"))
            {
                var areaParam = Material.ShaderParams["gsys_area_env_index_diffuse"];

                if (position != Vector3.Zero)
                {
                    var collectRes = LightingEngine.LightSettings.Resources.CollectFiles.FirstOrDefault().Value;
                    var area = collectRes.GetArea(position.X, position.Y, position.Z);
                    return area.AreaIndex;
                }

                float index = (float)areaParam.DataValue;
                return (int)index;
            }

            return 0;
        }

        private void UpdateMaterialBlock()
        {
            TexSrt texSrt0 = GetParameter<TexSrt>("tex_mtx0");
            TexSrt texSrt1 = GetParameter<TexSrt>("tex_mtx1");
            TexSrt texSrt2 = GetParameter<TexSrt>("tex_mtx2");
            float[] bake0ScaleBias = GetParameter<float[]>("gsys_bake_st0");
            float[] bake1ScaleBias = GetParameter<float[]>("gsys_bake_st1");
            float[] bakeLightScale = GetParameter<float[]>("gsys_bake_light_scale");
            float[] albedoColor = GetParameter<float[]>("albedo_tex_color");
            float[] emissionColor = GetParameter<float[]>("specular_color");
            float[] specularColor = GetParameter<float[]>("emission_color");

            float normalmapWeight = GetParameter<float>("normal_map_weight");
            float specularIntensity = GetParameter<float>("specular_intensity");
            float specularRoughness = GetParameter<float>("specular_roughness");
            float emissionIntensity = GetParameter<float>("emission_intensity");
            float transparency = GetParameter<float>("transparency");

            float[] multiTexReg0 = GetParameter<float[]>("multi_tex_reg0");
            float[] multiTexReg1 = GetParameter<float[]>("multi_tex_reg1");
            float[] multiTexReg2 = GetParameter<float[]>("multi_tex_reg2");
            float[] indirectMag = GetParameter<float[]>("indirect_mag");

            var mem = new System.IO.MemoryStream();
            using (var writer = new Toolbox.Core.IO.FileWriter(mem))
            {
                writer.Write(CalculateSRT3x4(texSrt0));
                writer.Write(bake0ScaleBias);
                writer.Write(bake1ScaleBias);
                writer.Write(CalculateSRT3x4(texSrt1));
                writer.Write(CalculateSRT3x4(texSrt2));

                writer.Write(albedoColor);
                writer.Write(transparency);

                writer.Write(emissionColor[0] * emissionIntensity);
                writer.Write(emissionColor[1] * emissionIntensity);
                writer.Write(emissionColor[2] * emissionIntensity);
                writer.Write(normalmapWeight);

                writer.Write(specularColor);
                writer.Write(specularIntensity);

                writer.Write(bakeLightScale);
                writer.Write(specularRoughness);

                writer.Write(multiTexReg0);
                writer.Write(multiTexReg1);
                writer.Write(multiTexReg2);

                writer.Write(indirectMag);
                writer.Write(new float[2]);
            }
            MaterialBlock.Buffer.Clear();
            MaterialBlock.Add(mem.ToArray());
        }

        private T GetParameter<T>(string name)
        {
            if (Material.AnimatedParams.ContainsKey(name))
                return (T)Material.AnimatedParams[name].DataValue;
            if (Material.ShaderParams.ContainsKey(name))
                return (T)Material.ShaderParams[name].DataValue;
            return (T)Activator.CreateInstance(typeof(T));
        }

        private float[] CalculateSRT3x4(BfresLibrary.TexSrt texSrt)
        {
            var m = CalculateSRT2x3(texSrt);
            return new float[12]
            {
                m[0], m[2], m[4], 0.0f,
                m[1], m[3], m[5], 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
            };
        }

        private float[] CalculateSRT2x3(BfresLibrary.TexSrt texSrt)
        {
            var scaling = texSrt.Scaling;
            var translate = texSrt.Translation;
            float cosR = (float)Math.Cos(texSrt.Rotation);
            float sinR = (float)Math.Sin(texSrt.Rotation);
            float scalingXC = scaling.X * cosR;
            float scalingXS = scaling.X * sinR;
            float scalingYC = scaling.Y * cosR;
            float scalingYS = scaling.Y * sinR;

            switch (texSrt.Mode)
            {
                default:
                case BfresLibrary.TexSrtMode.ModeMaya:
                    return new float[8]
                    {
                        scalingXC, -scalingYS, //0 1
                        scalingXS, scalingYC, // 4 5
                        -0.5f * (scalingXC + scalingXS - scaling.X) - scaling.X * translate.X, //12
                        -0.5f * (scalingYC - scalingYS + scaling.Y) + scaling.Y * translate.Y + 1.0f, //13
                        0.0f, 0.0f,
                    };
                case BfresLibrary.TexSrtMode.Mode3dsMax:
                    return new float[8]
                    {
                        scalingXC, -scalingYS,
                        scalingXS, scalingYC,
                        -scalingXC * (translate.X + 0.5f) + scalingXS * (translate.Y - 0.5f) + 0.5f, scalingYS * (translate.X + 0.5f) + scalingYC * (translate.Y - 0.5f) + 0.5f,
                        0.0f, 0.0f
                    };
                case BfresLibrary.TexSrtMode.ModeSoftimage:
                    return new float[8]
                    {
                        scalingXC, scalingYS,
                        -scalingXS, scalingYC,
                        scalingXS - scalingXC * translate.X - scalingXS * translate.Y, -scalingYC - scalingYS * translate.X + scalingYC * translate.Y + 1.0f,
                        0.0f, 0.0f,
                    };
            }
        }

        private float[] CalculateSRT(BfresLibrary.Srt2D texSrt)
        {
            var scaling = texSrt.Scaling;
            var translate = texSrt.Translation;
            float cosR = (float)Math.Cos(texSrt.Rotation);
            float sinR = (float)Math.Sin(texSrt.Rotation);

            return new float[8]
            {
                scaling.X * cosR, scaling.X * sinR,
                -scaling.Y * sinR, scaling.Y * cosR,
                translate.X, translate.Y,
                0.0f, 0.0f
            };
        }

        public Dictionary<string, GenericRenderer.TextureView> GetTextures()
        {
            return ParentRenderer.Textures;
        }

        /// <summary>
        /// Reloads render info and state settings into the materials blend state for rendering.
        /// </summary>
        public virtual void ReloadRenderState(Material mat, BfresMeshRender meshRender)
        {
            if (Material.ShaderOptions.ContainsKey("enable_color_buffer") && Material.ShaderOptions["enable_color_buffer"] == "1")
                meshRender.UseColorBufferPass = true;

            this.LightMap = BfresMatGLConverter.GetRenderInfo(mat, "gsys_light_diffuse");

            if (BfresMatGLConverter.GetRenderInfo(mat, "gsys_dynamic_depth_shadow") == "1")
                meshRender.ProjectDynamicShadowMap = true;  //Project shadows to cast onto objects
            if (BfresMatGLConverter.GetRenderInfo(mat, "gsys_static_depth_shadow") == "1")
                meshRender.ProjectStaticShadowMap = true; //Project shadows to cast onto objects (for map models)

            if (BfresMatGLConverter.GetRenderInfo(mat, "gsys_static_depth_shadow_only") == "1")
                meshRender.IsDepthShadow = true; //Only draw in the shadow pass.
            if (BfresMatGLConverter.GetRenderInfo(mat, "gsys_pass") == "seal")
                meshRender.IsSealPass = true; //Draw over objects
            if (BfresMatGLConverter.GetRenderInfo(mat, "gsys_cube_map_only") == "1")
                meshRender.IsCubeMap = true; //Draw only in cubemaps
            if (BfresMatGLConverter.GetRenderInfo(mat, "gsys_cube_map") == "1")
                meshRender.RenderInCubeMap = true; //Draw in cubemaps

            if (meshRender.IsDepthShadow || meshRender.IsCubeMap)
                meshRender.IsVisible = false;
        }

        static UVSphereRender MaterialSphere;

        public GLTexture RenderIcon(int size)
        {
            var context = new GLContext();
            context.Camera = new Camera();

            var frameBuffer = new Framebuffer(FramebufferTarget.Framebuffer, size, size);
            frameBuffer.Bind();

            GL.Viewport(0, 0, size, size);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Create a simple mvp matrix to render the material data
            Matrix4 modelMatrix = Matrix4.CreateTranslation(0, 0, -12);
            Matrix4 viewMatrix = Matrix4.Identity;
            Matrix4 mtxProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1.0f, 1.0f, 1000f);
            Matrix4 viewProj = mtxProj * viewMatrix;

            var mat = new StandardMaterial();
            mat.HalfLambertShading = true;
            mat.DirectionalLighting = false;
            mat.CameraMatrix = viewProj;
            mat.ModelMatrix = modelMatrix;
            mat.IsSRGB = true;

            var textures = this.GetTextures();
            for (int i = 0; i < Material.TextureMaps.Count; i++)
            {
                string name = Material.TextureMaps[i].Name;
                if (textures.ContainsKey(name) && Material.Samplers[i] == "_a0")
                    mat.DiffuseTextureID = textures[name].RenderTexture.ID;
            }

            if (MaterialSphere == null)
                MaterialSphere = new UVSphereRender(8);

            mat.Render(context);
            MaterialSphere.Draw(context);

            var thumbnail = frameBuffer.ReadImagePixels(true);

            //Disable shader and textures
            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.FramebufferSrgb);

            frameBuffer.Dispose();

            return GLTexture2D.FromBitmap(thumbnail);
        }

        public void ResetAnimations() {
            Material.AnimatedSamplers.Clear();
            Material.AnimatedParams.Clear();
        }

        public override void Render(GLContext control, ShaderProgram shader, GenericPickableMesh mesh) {
        }

        public virtual void RenderGBuffer(GLContext control, BfresRender parentRender, GenericPickableMesh mesh)
        {
            var shader = GlobalShaders.GetShader("DEBUG");
            control.CurrentShader = shader;

            DebugShaderRender.RenderMaterial(control);
            shader.SetInt("debugShading", (int)DebugShaderRender.DebugRender.Normal);

            SetRenderState();
        }

        public void RenderDefaultMaterials(GLContext control, GLTransform transform, ShaderProgram shader, GenericPickableMesh mesh)
        {
            if (Material.IsMaterialInvalid)
            {
                DrawEmptyMaterial(control, transform.TransformMatrix, (BfresMeshRender)mesh);
                return;
            }

            AreaIndex = GetAreaIndex(transform.Position);

            SetRenderState();
            SetBlendState();
            SetTextureUniforms(control, shader);
            SetShadows(control, shader);

            shader.SetFloat("uBrightness", Brightness);

            shader.SetBool("alphaTest", Material.BlendState.AlphaTest);
            shader.SetFloat("alphaRefValue", Material.BlendState.AlphaValue);
            shader.SetInt("alphaFunc", GetAlphaFunc(Material.BlendState.AlphaFunction));

            shader.SetBoolToInt("drawDebugAreaID", BfresRender.DrawDebugAreaID);
            shader.SetInt("areaID", AreaIndex);

           // UpdateMaterialBlock();
            MaterialBlock.RenderBuffer(shader.program, "ub_MaterialParams");
        }

        public void DrawEmptyMaterial(GLContext context, Matrix4 transform, BfresMeshRender mesh)
        {
            var mat = new StandardMaterial();
            mat.HalfLambertShading = true;
            mat.Color = new Vector4(1, 0.2f, 1, 1);
            //mat.DiffuseTextureID = RenderTools.defaultTex.ID;
            mat.ModelMatrix = transform;
            mat.Render(context);

            GL.Disable(EnableCap.CullFace);
            mesh.Draw(context.CurrentShader);
            GL.Enable(EnableCap.CullFace);
        }

        public void RenderDebugMaterials(GLContext control, BfresRender parentRender, ShaderProgram shader, GenericPickableMesh mesh)
        {
            AreaIndex = GetAreaIndex(parentRender.Transform.Position);

            SetRenderState();
            SetBlendState();
            SetTextureUniforms(control, shader);
            SetShadows(control, shader);

            DebugMaterial mat = new DebugMaterial();
            mat.Render(control, control.CurrentShader);

            shader.SetBool("alphaTest", Material.BlendState.AlphaTest);
            shader.SetFloat("alphaRefValue", Material.BlendState.AlphaValue);
            shader.SetInt("alphaFunc", GetAlphaFunc(Material.BlendState.AlphaFunction));

            shader.SetBoolToInt("drawDebugAreaID", BfresRender.DrawDebugAreaID);
            shader.SetInt("areaID", AreaIndex);

            shader.SetVector4("bake0_st", new Vector4(1, 1, 0, 0));
            shader.SetVector4("bake1_st", new Vector4(1, 1, 0, 0));
            shader.SetVector3("light_bake_scale", new Vector3(1, 1, 1));
            shader.SetInt("isNormalMapBC1", 1);
            if (Material.ShaderOptions.ContainsKey("gsys_normalmap_BC1"))
                shader.SetBoolToInt("isNormalMapBC1", Material.ShaderOptions["gsys_normalmap_BC1"] == "1");

            if (Material.ShaderParams.ContainsKey("gsys_bake_st0"))
            {
                float[] value = (float[])Material.ShaderParams["gsys_bake_st0"].DataValue;
                shader.SetVector4("bake0_st", new Vector4(value[0], value[1], value[2], value[3]));
            }
            if (Material.ShaderParams.ContainsKey("gsys_bake_st1"))
            {
                float[] value = (float[])Material.ShaderParams["gsys_bake_st1"].DataValue;
                shader.SetVector4("bake1_st", new Vector4(value[0], value[1], value[2], value[3]));
            }
            if (Material.ShaderParams.ContainsKey("gsys_bake_light_scale"))
            {
                float[] value = (float[])Material.ShaderParams["gsys_bake_light_scale"].DataValue;
                shader.SetVector3("light_bake_scale", new Vector3(value[0], value[1], value[2]));
            }

            shader.SetBool("hasProbes", false);
            if (parentRender.IsSelected)
                ProbeDebugger(parentRender, shader);

            // UpdateMaterialBlock();
            MaterialBlock.RenderBuffer(shader.program, "ub_MaterialParams");

            SetDebugTextureUniforms(control, shader);
        }

        private void ProbeDebugger(BfresRender parentRender, ShaderProgram shader)
        {
            bool debugProbeLighting = true;

            shader.SetBool("hasProbes", false);
            if (debugProbeLighting && Material.ShaderOptions.ContainsKey("gsys_enable_light_probe") && Material.ShaderOptions["gsys_enable_light_probe"] == "1")
            {
                shader.SetBool("hasProbes", true);
                ProbeMapManager.Generate(parentRender.Transform.Position, out float[] shData);
                Vector4[] probes = LightProbeMgr.ConvertSH2RGB(shData);

                for (int i = 0; i < probes.Length; i++)
                    GL.Uniform4(GL.GetUniformLocation(shader.program, String.Format("probeSH[{0}]", i)), probes[i]);
            }

        }

        static int GetAlphaFunc(AlphaFunction func)
        {
            if (func == AlphaFunction.Gequal) return 0;
            if (func == AlphaFunction.Greater) return 1;
            if (func == AlphaFunction.Equal) return 2;
            if (func == AlphaFunction.Less) return 3;
            if (func == AlphaFunction.Lequal) return 4;
            return 0;
        }

        public void RenderShadowMaterial(GLContext context)
        {
            context.CurrentShader.SetBoolToInt("hasAlpha", false);

            Material.BlendState.RenderAlphaTest();
            if (Material.BlendState.AlphaTest || Material.BlendState.BlendColor)
            {
                context.CurrentShader.SetBoolToInt("hasAlpha", true);
                SetTextureUniforms(context, context.CurrentShader);
            }
        }

        public virtual void SetShadows(GLContext control,ShaderProgram shader)
        {
            if (control.Scene.ShadowRenderer == null)
                return;

            var shadowRender = control.Scene.ShadowRenderer;

            var lightSpaceMatrix = shadowRender.GetLightSpaceViewProjMatrix();
            var shadowMap = shadowRender.GetProjectedShadow();
            var lightDir = shadowRender.GetLightDirection();

            shader.SetMatrix4x4("mtxLightVP", ref lightSpaceMatrix);
            shader.SetTexture(shadowMap, "shadowMap", 22);
            shader.SetVector3("lightDir", lightDir);
        }

        public virtual void SetBlendState()
        {
            Material.BlendState.RenderAlphaTest();
            Material.BlendState.RenderBlendState();
            Material.BlendState.RenderDepthTest();
        }

        public virtual void SetRenderState()
        {
            GL.Enable(EnableCap.CullFace);

            if (Material.CullFront && Material.CullBack)
                GL.CullFace(CullFaceMode.FrontAndBack);
            else if (Material.CullFront)
                GL.CullFace(CullFaceMode.Front);
            else if (Material.CullBack)
                GL.CullFace(CullFaceMode.Back);
            else
                GL.Disable(EnableCap.CullFace);
        }


        public virtual void SetDebugTextureUniforms(GLContext control, ShaderProgram shader)
        {
            shader.SetTexture(RenderTools.blackTex, "default", 5);

            int id = 5;
            for (int i = 0; i < Material.TextureMaps?.Count; i++)
            {
                var name = Material.TextureMaps[i].Name;
                var sampler = Material.TextureMaps[i].Sampler;
                //Lookup samplers targeted via animations and use that texture instead if possible
                if (Material.AnimatedSamplers.ContainsKey(sampler))
                    name = Material.AnimatedSamplers[sampler];

                string uniformName = "";

                //Only bind the required textures to save on rendering performance.
                if (DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.AO && sampler == "_b0")
                    uniformName = "AmbientOccusionBakeTexture";
                if (DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.Shadow && sampler == "_b0")
                    uniformName = "ShadowBakeTexture";
                if (DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.Lightmap ||
                    DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.LightmapAlpha && sampler == "_b1")
                {
                    uniformName = "IndirectLightBakeTexture";
                }
                if (DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.Emission && sampler == "_e0")
                    uniformName = "EmissionTexture";
                if (DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.Specular && sampler == "_s0")
                    uniformName = "SpecularTexture";
                if (DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.Diffuse && sampler == "_a0")
                    uniformName = "DiffuseTexture";
                if (DebugShaderRender.DebugRendering == DebugShaderRender.DebugRender.NormalMap && sampler == "_n0")
                    uniformName = "NormalMapTexture";

                if (uniformName == string.Empty)
                    continue;

                var binded = BindTexture(shader, sampler, GetTextures(), Material.TextureMaps[i], name, id);
                shader.SetInt(uniformName, id++);
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public virtual void SetTextureUniforms(GLContext control, ShaderProgram shader)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + 1);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex.ID);

            int id = 1;
            for (int i = 0; i < Material.TextureMaps?.Count; i++)
            {
                var name = Material.TextureMaps[i].Name;
                var sampler = Material.TextureMaps[i].Sampler;
                //Lookup samplers targeted via animations and use that texture instead if possible
                if (Material.AnimatedSamplers.ContainsKey(sampler))
                    name = Material.AnimatedSamplers[sampler];

                string uniformName = GetUniformName(sampler);
                if (sampler == "_a0")
                {
                    uniformName = "u_TextureAlbedo0";
                    shader.SetBoolToInt("hasDiffuseMap", true);
                }

                if (uniformName == string.Empty)
                    continue;

                var binded = BindTexture(shader, sampler, GetTextures(), Material.TextureMaps[i], name, id);
                shader.SetInt(uniformName, id++);
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        private string GetUniformName(string sampler)
        {
            switch (sampler)
            {
                case "_a0": return "u_TextureAlbedo0";
                case "_s0": return "u_TextureSpecMask";
                case "_n0": return "u_TextureNormal0";
                case "_n1": return "u_TextureNormal1";
                case "_e0": return "u_TextureEmission0";
                case "_b0": return "u_TextureBake0";
                case "_b1": return "u_TextureBake1";
                case "_a1": return "u_TextureMultiA";
                case "_a2": return "u_TextureMultiB";
                case "_a3": return "u_TextureIndirect";
                default:
                    return "";
            }
        }

        public static GLTexture BindTexture(ShaderProgram shader, string sampler, Dictionary<string, GenericRenderer.TextureView> textures,
            STGenericTextureMap textureMap, string name, int id)
        {
            if (name == null)
                return null;

            GL.ActiveTexture(TextureUnit.Texture0 + id);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex.ID);

            if (textures.ContainsKey(name))
                return BindGLTexture(textures[name], textureMap, sampler);

            foreach (var model in DataCache.ModelCache.Values)
            {
                if (model.Textures.ContainsKey(name))
                    return BindGLTexture(model.Textures[name], textureMap, sampler);
            }

            return RenderTools.defaultTex;
        }

        private static GLTexture BindGLTexture(GenericRenderer.TextureView texture, STGenericTextureMap textureMap, string sampler)
        {
            if (texture.RenderTexture == null)
                return null;

            var target = ((GLTexture)texture.RenderTexture).Target;

            GL.BindTexture(target, texture.RenderTexture.ID);
            GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)OpenGLHelper.WrapMode[textureMap.WrapU]);
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)OpenGLHelper.WrapMode[textureMap.WrapV]);
            GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)OpenGLHelper.MinFilter[textureMap.MinFilter]);
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)OpenGLHelper.MagFilter[textureMap.MagFilter]);
            GL.TexParameter(target, TextureParameterName.TextureLodBias, textureMap.LODBias);
            GL.TexParameter(target, TextureParameterName.TextureMaxLod, textureMap.MaxLOD);
            GL.TexParameter(target, TextureParameterName.TextureMinLod, textureMap.MinLOD);

            int[] mask = new int[4]
            {
                    OpenGLHelper.GetSwizzle(texture.RedChannel),
                    OpenGLHelper.GetSwizzle(texture.GreenChannel),
                    OpenGLHelper.GetSwizzle(texture.BlueChannel),
                    OpenGLHelper.GetSwizzle(texture.AlphaChannel),
            };
            GL.TexParameter(target, TextureParameterName.TextureSwizzleRgba, mask);
            return (GLTexture)texture.RenderTexture;
        }
    }
}
