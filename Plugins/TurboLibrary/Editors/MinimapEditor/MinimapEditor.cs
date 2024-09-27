using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapStudio.UI;
using Toolbox.Core;
using Toolbox.Core.ViewModels;
using ImGuiNET;
using GLFrameworkEngine;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.IO;
using SixLabors.ImageSharp;

namespace TurboLibrary.MuuntEditor
{
    public class MinimapEditor : FileEditor, IFileFormat
    {
        public string[] Description => new string[] { "bin" };
        public string[] Extension => new string[] { "*.bin" };

        /// <summary>
        /// Wether or not the file can be saved.
        /// </summary>
        public bool CanSave { get; set; } = true;

        /// <summary>
        /// Information of the loaded file.
        /// </summary>
        public File_Info FileInfo { get; set; } = new File_Info();

        private STGenericTexture MinmapTexture;
        private MapCamera CameraParams;
        private Camera _camera;
        private MinimapRender Renderer = new MinimapRender();
        private MinimapTextureGen minimapTextureGen = new MinimapTextureGen();

        static float Transparency = 0.5f;

        public MinimapEditor()
        {
        }

        private void Init()
        {
            _camera = new Camera();
            _camera.RotationSpeed = 0.2f;
            _camera.PanSpeed = 100000;
            ((InspectCameraController)_camera.Controller).PanDirectly = true;
        }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            return false;
        }

        public void Load(Stream stream) {
            Load(new MapCamera(stream), null);
        }

        public void Save(Stream stream) {
            CameraParams.Save(stream);
        }

        public void Load(MapCamera cameraParams, STGenericTexture texture) {

            if (reloaded)
                return;

            Init();
            CameraParams = cameraParams;
            MinmapTexture = texture;
            Renderer.Texture = texture;

            if (!init)
                Reload();
        }

        private bool init = false;

        private void Reload()
        {
            AddRender(Renderer);

            Scene.Init();
            Scene.Cursor3D.IsVisible = false;

            Root.Children.Clear();

            var textureNode = new NodeBase("course_maptexture.bflim");
            textureNode.Tag = MinmapTexture;

            Root.Header = "course_mapcamera.bin";
            Root.Tag = CameraParams;
            Root.TagUI.UIDrawer += delegate {
                RenderCameraProperties();
            };

            Root.AddChild(textureNode);

            Root.ContextMenus.Clear();
            Root.ContextMenus.Add(new MenuItemModel("Save", Save));

            init = true;
        }

        public void Save()
        {
            ImguiFileDialog dlg = new ImguiFileDialog();
            dlg.SaveDialog = true;
            dlg.FileName = "course_mapcamera.bin";
            dlg.AddFilter(".bin", "bin");
            if (dlg.ShowDialog()) {
                //Apply camera params
                _camera.UpdateMatrices();
                CameraParams.Position = _camera.GetViewPostion();
                CameraParams.LookAtPosition = _camera.GetLookAtPostion(1000);
                CameraParams.Save(dlg.FilePath);
            }
        }

        bool reloaded = false;

        public void ReloadEditor(GLContext context) {
            context.Camera = _camera;
            UpdateCamera();
        }

        private void ApplyCameraPlacement()
        {
            _camera.TargetPosition = CameraParams.Position;
            _camera.UpdateMatrices();
        }

        private void RenderCameraProperties()
        {
            bool edited = false;

            if (ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen))
            {
                bool updateCamera = false;
                updateCamera |= ImGuiHelper.InputTKVector3("Position", _camera, "TargetPosition");
                updateCamera |= ImGuiHelper.InputFromFloat("Rotation X", _camera, "RotationDegreesX", true, 0.1F);
                updateCamera |= ImGuiHelper.InputFromFloat("Rotation Y", _camera, "RotationDegreesY", true, 0.1F);
                updateCamera |= ImGuiHelper.InputFromFloat("Rotation Z", _camera, "RotationDegreesZ", true, 0.1F);
                if (updateCamera) {
                    _camera.UpdateMatrices();
                    CameraParams.Position = _camera.GetViewPostion();
                    CameraParams.LookAtPosition = _camera.GetLookAtPostion(1000);

                    ApplyCameraPlacement();
                    //  minimapTextureGen.Update(Workspace);
                }
            }
            if (ImGui.CollapsingHeader("Zoom", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGuiHelper.InputFromFloat("Ortho Width", CameraParams, "Width", false, 200)) {
                    //uniform scaling
                    CameraParams.Height = CameraParams.Width;
                    _camera.ProjectionMatrix = CameraParams.ConstructOrthoMatrix();
                    _camera.UpdateMatrices();
                }
                if (ImGuiHelper.InputFromFloat("Ortho Height", CameraParams, "Height", false, 200)) {
                    //uniform scaling
                    CameraParams.Width = CameraParams.Height;
                    _camera.ProjectionMatrix = CameraParams.ConstructOrthoMatrix();
                    _camera.UpdateMatrices();
                }
            }

            if (ImGui.CollapsingHeader("Viewer", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.BoldTextLabel("Pan Camera:", "Shift + Left Mouse");
                ImGuiHelper.BoldTextLabel("Rotate Camera:", "Drag Left Mouse");

                if (ImGui.DragFloat("Transparency", ref Transparency, 0.01f, 0.01f, 1)) {
                    GLContext.ActiveContext.UpdateViewport = true;
                }

                var width = ImGui.GetWindowWidth();
                
                if (ImGui.Button("Take Screenshot", new System.Numerics.Vector2(width, 30)))
                {
                    Renderer.IsVisible = false;

                    var thumb = Workspace.ViewportWindow.SaveAsScreenshot(1024, 1024, true);
                    var dlg = new ImguiFileDialog();
                    dlg.SaveDialog = true;
                    dlg.FileName = "course_maptexture.png";
                    if (dlg.ShowDialog())
                    {
                        thumb.SaveAsPng(dlg.FilePath);
                    }
                    Renderer.IsVisible = true;
                }

                if (ImGui.Button("Save Camera", new System.Numerics.Vector2(width, 23))) {
                    Save();
                }
                if (ImGui.Button("Save Minimap Texture", new System.Numerics.Vector2(width, 23)))
                {
                    if (MinmapTexture is BflimTexture)
                        ((BflimTexture)MinmapTexture).SaveDialog();
                }
            }

           /* if (ImGui.CollapsingHeader("Texture Generator"))
            {
                ImGui.ColorEdit3("Color", ref minimapTextureGen.Color);
                minimapTextureGen.Draw();
            }*/
            if (ImGui.CollapsingHeader("Camera Speed"))
            {
                if (ImGuiHelper.InputFromFloat("Rotation Speed", _camera, "RotationSpeed", true, 0.01f)) {
                    GLContext.ActiveContext.UpdateViewport = true;
                }
                if (ImGuiHelper.InputFromFloat("Pan Speed", _camera, "PanSpeed", true, 10)) {
                    GLContext.ActiveContext.UpdateViewport = true;
                }
            }
            if (ImGui.CollapsingHeader("Advanced"))
            {
                edited |= ImGuiHelper.InputTKVector3("Position##paramPos", CameraParams, "Position", 20);
                edited |= ImGuiHelper.InputTKVector3("Look At##paramAt", CameraParams, "LookAtPosition", 20);
                edited |= ImGuiHelper.InputTKVector3("Up Axis##paramUp", CameraParams, "UpAxis");
            }

            if (edited)
            {
                UpdateCamera();
                GLContext.ActiveContext.UpdateViewport = true;
            }
        }

        private void UpdateCamera()
        {
            _camera.CanUpdate = true;

            _camera.UseSquareAspect = true;

            _camera.Distance = 0;
            _camera.TargetDistance = 0;
            _camera.TargetPosition = CameraParams.Position;
            _camera.RotateFromLookat(CameraParams.Position, CameraParams.LookAtPosition, CameraParams.UpAxis);
            _camera.UpdateMatrices();

            _camera.ProjectionMatrix = CameraParams.ConstructOrthoMatrix();
            _camera.CanUpdate = false;
        }

        class MinimapRender : IDrawable
        {
            public STGenericTexture Texture;

            public bool IsVisible { get; set; } = true;

            RenderMesh<VertexPositionTexCoord> RenderMesh;

            public void DrawModel(GLContext context, Pass pass)
            {
                if (pass != Pass.TRANSPARENT || Texture == null || Texture.RenderableTex == null)
                    return;

                GL.Disable(EnableCap.DepthTest);

                if (RenderMesh == null)
                    RenderMesh = new RenderMesh<VertexPositionTexCoord>(Vertices, PrimitiveType.TriangleStrip);

                var mat = new StandardMaterial();
                mat.IsSRGB = true;
                mat.Color = new Vector4(1, 1, 1, Transparency);
                mat.DiffuseTextureID = Texture.RenderableTex.ID;
                //Fit to screen.
                mat.CameraMatrix = Matrix4.Identity;

                GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();
                mat.Render(context);

                RenderMesh.Draw(context);

                GL.Enable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GLMaterialBlendState.Opaque.RenderBlendState();
            }

            public void Dispose()
            {
            }

            static VertexPositionTexCoord[] Vertices => new VertexPositionTexCoord[]
            {
                   new VertexPositionTexCoord(new Vector3(-1.0f, 1.0f, 0.0f), new Vector2(0, 0)),
                   new VertexPositionTexCoord(new Vector3(-1.0f, -1.0f, 0), new Vector2(0, 1)),
                   new VertexPositionTexCoord(new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1, 0)),
                   new VertexPositionTexCoord(new Vector3(1.0f, -1.0f,0.0f), new Vector2(1, 1)),
           };
        }
    }
}
