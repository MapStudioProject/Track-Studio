using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AampLibraryCSharp;
using OpenTK.Graphics.OpenGL;
using GLFrameworkEngine;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class LightingEngine
    {
        public static LightingEngine LightSettings = new LightingEngine();

        public EventHandler CubeMapsUpdate;

        //A lookup of lightmaps per object to get probe lighting (made from static light map base)
        public Dictionary<string, GLTextureCube> ProbeLightmaps = new Dictionary<string, GLTextureCube>();

        public GraphicFileParamResources Resources = new GraphicFileParamResources();

        public GLTexture3D ColorCorrectionTable;
        public GLTexture2DArray LightPrepassTexture;
        public GLTexture2D ShadowPrepassTexture;

        public bool UpdateColorCorrection = false;
        public bool UpdateCubemaps = false;

        public void LoadArchive(List<ArchiveFileInfo> files, bool isWiiU = false)
        {
            Resources.LoadArchive(files, isWiiU);
            InitTextures();

            if (!this.Resources.EnvFiles.ContainsKey("course_area.baglenv"))
                return;

            var dir = this.Resources.EnvFiles["course_area.baglenv"].DirectionalLights[0];

            GLContext.ActiveContext.Scene.LightDirection = new OpenTK.Vector3(
               dir.Direction.X, dir.Direction.Y, dir.Direction.Z);
        }

        public void SaveArchive(Dictionary<string, byte[]> files)
        {
            Resources.SaveArchive(files);
        }

        public void InitTextures()
        {
            if (LightPrepassTexture != null)
                return;

            //Used for dynamic lights. Ie spot, point, lights
            //Dynamic lights are setup using the g buffer pass (normals) and depth information before material pass is drawn
            //Additional slices may be used for bloom intensity
            LightPrepassTexture = GLTexture2DArray.CreateConstantColorTexture(4, 4, 1, 0, 0, 0, 0);

            LightPrepassTexture.Bind();
            LightPrepassTexture.MagFilter = TextureMagFilter.Linear;
            LightPrepassTexture.MinFilter = TextureMinFilter.Linear;
            LightPrepassTexture.UpdateParameters();
            LightPrepassTexture.Unbind();

            //Shadows
            //Channel usage:
            //Red - Dynamic shadows
            //Green - Static shadows (for casting onto objects)
            //Blue - Soft shading (dynamic AO)
            //Alpha - decals
            ShadowPrepassTexture = GLTexture2D.CreateWhiteTexture(4, 4);

            foreach (var lmap in Resources.LightMapFiles.Values)
                lmap.Setup();
        }

        public void UpdateColorCorrectionTable()
        {
            if (ColorCorrectionTable == null)
            {
                ColorCorrectionTable = GLTexture3D.CreateUncompressedTexture(8, 8, 8,
                    PixelInternalFormat.R11fG11fB10f, PixelFormat.Rgb, PixelType.UnsignedInt10F11F11FRev);

                ColorCorrectionTable.Bind();
                ColorCorrectionTable.MagFilter = TextureMagFilter.Linear;
                ColorCorrectionTable.MinFilter = TextureMinFilter.Linear;
                ColorCorrectionTable.UpdateParameters();
                ColorCorrectionTable.Unbind();
            }

            ColorCorrectionManager3D.CreateColorLookupTexture(ColorCorrectionTable);

            MapStudio.UI.DeferredRenderQuad.LUTTexture = ColorCorrectionTable;
            UpdateColorCorrection = false;
            GLContext.ActiveContext.UpdateViewport = true;
        }

        public void UpdateLightPrepass(GLContext control, GLTexture gbuffer, GLTexture linearDepth) {
            if (LightPrepassTexture == null) {
                LightPrepassTexture = GLTexture2DArray.CreateUncompressedTexture(control.Width, control.Height, 1, 1,
                     PixelInternalFormat.R11fG11fB10f, PixelFormat.Rgb, PixelType.UnsignedInt10F11F11FRev);
                LightPrepassTexture.Bind();
                LightPrepassTexture.MagFilter = TextureMagFilter.Linear;
                LightPrepassTexture.MinFilter = TextureMinFilter.Linear;
                LightPrepassTexture.UpdateParameters();
                LightPrepassTexture.Unbind();
            }
           // LightPrepassManager.CreateLightPrepassTexture(control, gbuffer, linearDepth, LightPrepassTexture);
        }

        public void ResetShadowPrepass()
        {
            if (ShadowPrepassTexture != null)
                ShadowPrepassTexture.Dispose();

            ShadowPrepassTexture = GLTexture2D.CreateWhiteTexture(4, 4);
        }

        public void UpdateShadowPrepass(GLContext control, GLTexture shadowMap, GLTexture normalsTexture, GLTexture depthTexture)
        {
            if (ShadowPrepassTexture == null)
            {
                ShadowPrepassTexture = GLTexture2D.CreateUncompressedTexture(control.Width, control.Height,
                     PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte);
            }
            ShadowPrepassManager.CreateShadowPrepassTexture(control, shadowMap, normalsTexture, depthTexture, ShadowPrepassTexture);
        }

        public void UpdateCubemap(List<GenericRenderer> renders, bool isWiiU) {
            CubemapManager.GenerateCubemaps(renders, isWiiU);
        }

        public void UpdateAllLightMaps(GLContext context)
        {
            //Also update lighting direction in the scene
            var dir = this.Resources.EnvFiles["course_area.baglenv"].DirectionalLights[0];

            context.Scene.LightDirection = new OpenTK.Vector3(
               dir.Direction.X, dir.Direction.Y, dir.Direction.Z);

            foreach (var lightmap in Resources.LightMapFiles.FirstOrDefault().Value.Lightmaps.Keys)
                UpdateLightmap(context, lightmap);
        }

        public void UpdateLightmap(GLContext control, string lightMapName)
        {
            var lmap = Resources.LightMapFiles.FirstOrDefault().Value;
            var env = Resources.EnvFiles["course_area.baglenv"];

            lmap.GenerateLightmap(control, env, lightMapName);
        }

        public GLTextureCube UpdateProbeCubemap(GLContext control, GLTextureCube probeMap, OpenTK.Vector3 position)
        {
            if (probeMap == null)
            {
                probeMap = GLTextureCube.CreateEmptyCubemap(
                       32, PixelInternalFormat.Rgb32f, PixelFormat.Rgb, PixelType.Float, 2);

                probeMap.Bind();
                probeMap.MagFilter = TextureMagFilter.Linear;
                probeMap.MinFilter = TextureMinFilter.LinearMipmapLinear;
                probeMap.UpdateParameters();
                probeMap.Unbind();
            }

            var lmap = Resources.LightMapFiles.FirstOrDefault().Value;
            var collectRes = Resources.CollectFiles.FirstOrDefault().Value;

            //Find the area to get the current light map
            var areaObj = collectRes.GetArea(position.X, position.Y, position.Z);
            var lightMapName = areaObj.GetLightmapName();

            if (!lmap.Lightmaps.ContainsKey(lightMapName))
                UpdateLightmap(control, lightMapName);

            ProbeMapManager.Generate(control, (GLTextureCube)lmap.Lightmaps[lightMapName], probeMap.ID, position);
            return probeMap;
        }
    }
}
