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
    /// <summary>
    /// A manager for keeping track of the scene's cubemaps.
    /// This will render cubemaps in the current scene given the list of renderable objects.
    /// </summary>
    public class CubemapManager
    {
        public static GLTexture CubeMapTexture { get; set; }
        public static GLTexture CubeMapTextureArray { get; set; }

        //Size of inital cubemap size
        public const int CUBEMAP_UPSCALE_SIZE = 256;
        //Size of output cubemap with HDR applied
        public const int CUBEMAP_SIZE = 128;
        //Array count for multiple cubemaps per area
        public const int MAX_LAYER_COUNT = 8;

        //Keep things simple and use single mips for now then generate later
        public const int MAX_MIP_LEVEL = 1;
        //Debug save to disc for viewing
        public const bool SAVE_TO_DISK = false;

        public CubemapManager()
        {

        }

        //Sets a default cubemap for viewing
        public static void InitDefault(GLTextureCubeArray texture)
        {
            if (CubeMapTexture == null)
                CubeMapTexture = texture;
        }

        //Update all existing cubemap uint objects
        public static void GenerateCubemaps(List<GenericRenderer> targetModels, bool isWiiU)
        {
            //Wii U requires 2D arrays atm (decaf decompiler only supports 2d arrays vs cubemap samplers)
            var texture = isWiiU ? CubeMapTextureArray : CubeMapTexture;

            if (texture != null)
                texture.Dispose();

            if (isWiiU)
            {
                texture = GLTexture2DArray.CreateUncompressedTexture(CUBEMAP_SIZE, CUBEMAP_SIZE, MAX_LAYER_COUNT * 6, MAX_MIP_LEVEL,
                    PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float);
            }
            else
            {
                texture = GLTextureCubeArray.CreateEmptyCubemap(CUBEMAP_SIZE, MAX_LAYER_COUNT, MAX_MIP_LEVEL,
                    PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float);
            }

            GLTextureCube cubemapTexture = GLTextureCube.CreateEmptyCubemap(
                CUBEMAP_UPSCALE_SIZE, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, 9);

            //Get a list of cubemaps in the scene
            //The lighting engine has cube map objects with the object placement to draw
            var lightingEngine = LightingEngine.LightSettings;
            var cubemapEnvParams = lightingEngine.Resources.CubeMapFiles.FirstOrDefault().Value;
            var cubeMapUints = cubemapEnvParams.CubeMapObjects;
            int layer = 0;
            foreach (var cubeMap in cubeMapUints)
            {
                var cUint = cubeMap.CubeMapUint;
                //Cubemap has no area assigned skip it
                if (cubeMap.CubeMapUint.Name == string.Empty || MAX_LAYER_COUNT <= layer)
                    continue;

                //Setup the camera to render the cube map faces
                CubemapCamera camera = new CubemapCamera(
                    new Vector3(cUint.Position.X, cUint.Position.Y, cUint.Position.Z)
                    * GLContext.PreviewScale, cUint.Near, cUint.Far);

                var context = new GLContext();
                context.Camera = camera;
                context.Scene.Init();

                GenerateCubemap(context, cubemapTexture, camera, targetModels, MAX_MIP_LEVEL);

                cubemapTexture.Bind();
                cubemapTexture.GenerateMipmaps();
                cubemapTexture.Unbind();

                //HDR encode and output into the array
                CubemapHDREncodeRT.CreateCubemap(cubemapTexture, texture, layer, MAX_MIP_LEVEL, false, true);

                layer++;
            }

            cubemapTexture.Dispose();

            //Just generate mips to keep things easier
            texture.Bind();
            texture.GenerateMipmaps();
            texture.Unbind();

            if (SAVE_TO_DISK)
                texture.SaveDDS("Cubemap_Array_HDR.dds");

            if (isWiiU)
                CubeMapTextureArray = texture;
            else
                CubeMapTexture = texture;
        }

        static void GenerateCubemap(GLContext control, GLTextureCube texture,
             CubemapCamera camera, List<GenericRenderer> models, int numMips)
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            int size = CUBEMAP_UPSCALE_SIZE;

            Framebuffer frameBuffer = new Framebuffer(FramebufferTarget.Framebuffer, size, size, PixelInternalFormat.Rgba16f);
            frameBuffer.SetDrawBuffers(DrawBuffersEnum.ColorAttachment0);
            frameBuffer.Bind();

            //Render all 6 faces
            for (int mip = 0; mip < numMips; mip++)
            {
                int mipWidth = (int)(size * Math.Pow(0.5, mip));
                int mipHeight = (int)(size * Math.Pow(0.5, mip));

                frameBuffer.Resize(mipWidth, mipHeight);
                GL.Viewport(0, 0, mipWidth, mipHeight);

                for (int i = 0; i < 6; i++)
                {
                    //First filter a normal texture 2d face
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                                   FramebufferAttachment.ColorAttachment0,
                                    TextureTarget.TextureCubeMapPositiveX + i, texture.ID, mip);

                    //point camera in the right direction
                    camera.SwitchToFace(i);

                    GL.ClearColor(0, 0, 0, 1);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    //render scene to fbo, and therefore to the current face of the cubemap
                    foreach (var model in models)
                        model.DrawCubeMapScene(control);
                }
            }

            var status = frameBuffer.GetStatus();
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception(status.ToString());

            frameBuffer.Dispose();
            frameBuffer.DisposeRenderBuffer();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.UseProgram(0);
        }
    }

    public class CubemapCamera : Camera
    {
        public CubemapCamera(Vector3 center, float znear = 1, float zfar = 500000)
        {
            TargetPosition = center;
            ZNear = znear;
            ZFar = zfar;
            FovDegrees = 90.0f; //Cubemap camera should have an fov of 90 degrees
            //Just need a square 1.0 aspect ratio
            Width = 1;
            Height = 1;
            this.UpdateMatrices();
        }

        /// <summary>   
        /// Switches the camera to face the given face index direction when rendering a cubemap face.
        /// </summary>
        public void SwitchToFace(int faceIndex)
        {
            switch (faceIndex)
            {
                case 0:
                    RotationX = 0;
                    RotationDegreesY = 90;
                    break;
                case 1:
                    RotationDegreesX = 0;
                    RotationDegreesY = -90;
                    break;
                case 2:
                    RotationDegreesX = 90;
                    RotationDegreesY = 0;
                    break;
                case 3:
                    RotationDegreesX = -90;
                    RotationDegreesY = 0;
                    break;
                case 4:
                    RotationDegreesX = 0;
                    RotationDegreesY = 0;
                    break;
                case 5:
                    RotationDegreesX = 0;
                    RotationDegreesY = 180;
                    break;
            }
            this.UpdateMatrices();
        }
    }
}