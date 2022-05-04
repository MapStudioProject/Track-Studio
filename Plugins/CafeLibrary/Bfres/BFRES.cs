using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;
using Toolbox.Core;
using Toolbox.Core.Hashes;
using BfresLibrary;
using MapStudio.UI;
using CafeLibrary.Rendering;
using Syroot.NintenTools.NSW.Bntx;
using OpenTK;
using ImGuiNET;
using UIFramework;

namespace CafeLibrary
{
    public class BFRES : FileEditor, IFileFormat, IDisposable
    {
        public string[] Description => new string[] { "bfres" };

        public string[] Extension => new string[] { ".bfres", ".szs", ".sbfres" };

        public bool CanSave { get; set; } = true;
        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new Toolbox.Core.IO.FileReader(stream, true)) {
                return reader.CheckSignature(4, "FRES");
            }
        }

        //renderer
        public BfresRender Renderer;

        //Tree Nodes
        public ModelFolder ModelFolder;
        public TextureFolder TextureFolder;
        public AnimationsFolder AnimationsFolder;
        public EmbeddedFilesFolder EmbeddedFilesFolder;

        public ModelImportSettings ImportSettings = new ModelImportSettings();

        public ResFile ResFile { get; set; }

        public bool UpdateTransformedVertices = false;

        public void Load(Stream stream)
        {
            ResFile = new ResFile(stream);
            Reload();
        }

        public void TryInsertBoneKey(STBone bone, BfresSkeletalAnim.InsertFlags flags)
        {
            var anim = this.Workspace.GetActiveAnimation();
            if (anim != null && anim is BfresSkeletalAnim)
                ((BfresSkeletalAnim)anim).InsertBoneKey(bone, flags);
            else
                TinyFileDialog.MessageBoxInfoOk($"A skeletal animation must be active to insert keys! Select one to activate.");
        }

        public void TryInsertParamKey(string material, ShaderParam param)
        {
            var anim = this.Workspace.GetActiveAnimation();
            if (anim != null && anim is BfresMaterialAnim)
                ((BfresMaterialAnim)anim).InsertParamKey(material, param);
            else
                TinyFileDialog.MessageBoxInfoOk($"A shader param animation must be active to insert keys! Select one to activate.");
        }

        public void TryInsertTextureKey(string material, string sampler, string texture)
        {
            var anim = this.Workspace.GetActiveAnimation();
            if (anim != null && anim is BfresMaterialAnim)
                ((BfresMaterialAnim)anim).InsertTextureKey(material, sampler, texture);
            else
                TinyFileDialog.MessageBoxInfoOk($"A texture pattern animation must be active to insert keys! Select one to activate.");
        }

        public override bool CreateNew()
        {
            ResFile = new ResFile();

            FileInfo = new File_Info();
            FileInfo.FilePath = "NewFile";
            FileInfo.FileName = "NewFile";
            FileInfo.Compression = new Yaz0();
            ResFile.Name = "NewFile";

            this.Root.Header = "NewFile.szs";
            this.Root.Tag = this;

            Reload();
            return true;
        }

        public override void RenderSaveFileSettings()
        {
            var comp = FileInfo.Compression == null ? "None" : FileInfo.Compression.ToString();
            if (ImGui.BeginCombo("Compression", comp))
            {
                bool select = comp == "None";
                if (ImGui.Selectable("None", select)) {
                    FileInfo.Compression = null;
                }
                if (select)
                    ImGui.SetItemDefaultFocus();

                foreach (var compTypes in Toolbox.Core.FileManager.GetCompressionFormats())
                {
                    select = comp == compTypes.ToString();
                    if (ImGui.Selectable(compTypes.ToString(), select)) {
                        FileInfo.Compression = compTypes;
                    }

                    if (select)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (ImGui.DragInt("Yaz0 Level (1 fast, 9 slow)", ref Runtime.Yaz0CompressionLevel, 1, 1, 9)) {

            }
        }

        private void Reload()
        {
            Renderer = new BfresRender(this);
            Renderer.Name = FileInfo.FilePath;
            Renderer.CanSelect = false;
            Renderer.MeshPicking = true;
            Renderer.Transform.TransformUpdated += delegate
            {
                this.UpdateTransformedVertices = true;
            };

            Root.Tag = this;
            Root.TagUI.UIDrawer += delegate
            {
                ImguiBinder.LoadPropertiesComponentModelBase(ResFile);
            };
            BfresLoader.LoadShaders(ResFile, Renderer);

            BntxFile bntx = TextureFolder.CreateBntx();
            if (ResFile.IsPlatformSwitch)
            {
                foreach (var file in ResFile.ExternalFiles.Values)
                    if (file.LoadedFileData is BntxFile)
                        bntx = file.LoadedFileData as BntxFile;
            }

            TextureFolder = new TextureFolder(ResFile, bntx);
            ModelFolder = new ModelFolder(this, ResFile);
            AnimationsFolder = new AnimationsFolder(this, ResFile);
            EmbeddedFilesFolder = new EmbeddedFilesFolder(ResFile);

            foreach (var texNode in TextureFolder.Children)
            {
                var tex = texNode.Tag as STGenericTexture;
                Renderer.Textures.Add(tex.Name, new GenericRenderer.TextureView(tex) { OriginalSource = tex });
            }

            //Using events for when the textures are updated
            //This is to help seperate editing vs rendering
            TextureFolder.OnTextureAdded += (o, e) =>
            {
                var node = o as TextureNode;
                var tex = node.Tag as STGenericTexture;
                Renderer.Textures.Add(node.Header,
                    new GenericRenderer.TextureView(tex) { OriginalSource = tex });
                GLContext.ActiveContext.UpdateViewport = true;
            };
            TextureFolder.OnTextureEdited += (o, e) =>
            {
                var node = o as TextureNode;
                var tex = node.Tag as STGenericTexture;
                Renderer.Textures[node.Header].RedChannel = tex.RedChannel;
                Renderer.Textures[node.Header].GreenChannel = tex.GreenChannel;
                Renderer.Textures[node.Header].BlueChannel = tex.BlueChannel;
                Renderer.Textures[node.Header].AlphaChannel = tex.AlphaChannel;
                GLContext.ActiveContext.UpdateViewport = true;
            };
            TextureFolder.OnTextureRemoved += (o, e) =>
            {
                var node = o as TextureNode;
                Renderer.Textures[node.Header]?.RenderTexture?.Dispose();
                Renderer.Textures.Remove(node.Header);
                GLContext.ActiveContext.UpdateViewport = true;
            };
            TextureFolder.OnTextureRenamed += (o, e) =>
            {
                var node = o as TextureNode;
                var tex = node.Tag as STGenericTexture;
                //Update the lookup
                Renderer.Textures.Remove(tex.Name);
                Renderer.Textures.Add(node.Header,
                    new GenericRenderer.TextureView(tex) { OriginalSource = tex });
                GLContext.ActiveContext.UpdateViewport = true;
            };
            TextureFolder.OnTextureReplaced += (o, e) =>
            {
                var node = o as TextureNode;
                var tex = node.Tag as STGenericTexture;

                Renderer.Textures[node.Header]?.RenderTexture?.Dispose();
                Renderer.Textures.Remove(node.Header);

                Renderer.Textures.Add(node.Header,
                    new GenericRenderer.TextureView(tex) { OriginalSource = tex });
                GLContext.ActiveContext.UpdateViewport = true;
            };

            Root.AddChild(ModelFolder);
            Root.AddChild(TextureFolder);
            Root.AddChild(AnimationsFolder);
            Root.AddChild(EmbeddedFilesFolder);

            AddRender(Renderer);
            //Add skeleton objects
            foreach (BfresModelRender model in Renderer.Models)
                AddRender(model.SkeletonRenderer);

            if (!DataCache.ModelCache.ContainsKey(Renderer.Name))
                DataCache.ModelCache.Add(Renderer.Name, Renderer);
        }

        private string GetSampler(string textureName)
        {
            foreach (var model in ResFile.Models.Values) {
                foreach (var mat in model.Materials.Values) {
                    for (int i = 0; i < mat.TextureRefs.Count; i++)
                    {
                        if (mat.TextureRefs[i].Name == textureName)
                            return mat.Samplers[i].Name;
                    }
                }
            }
            return "";
        }

        public override void PrintErrors()
        {
            base.PrintErrors();

            ModelFolder.CheckErrors();
            TextureFolder.CheckErrors();
        }

        /// <summary>
        /// Prepares the dock layouts to be used for the file format.
        /// </summary>
        public override List<DockWindow> PrepareDocks()
        {
            List<DockWindow> windows = new List<DockWindow>();
            windows.Add(Workspace.Outliner);
            windows.Add(Workspace.PropertyWindow);
            windows.Add(Workspace.ConsoleWindow);
            windows.Add(Workspace.AssetViewWindow);
            windows.Add(Workspace.HelpWindow);
            windows.Add(Workspace.ViewportWindow);
            windows.Add(Workspace.TimelineWindow);
            windows.Add(Workspace.GraphWindow);
            windows.Add(Workspace.UVWindow);
            return windows;
        }

        public void ExportTextures(string folder)
        {
            TextureFolder.ExportAllTextures(folder);
        }


        public override bool OnFileDrop(string filePath)
        {
            if (filePath.EndsWith(".png") || 
                filePath.EndsWith(".dds") ||
                filePath.EndsWith(".bftex"))
            {
                TextureFolder.ImportTexture(filePath);
                return true;
            }
            if (filePath.EndsWith(".bfmat") || filePath.EndsWith(".zip"))
            {
                ModelFolder.TargtModel.AddMaterialDialog(filePath);
                return true;
            }
            if (filePath.EndsWith(".dae") ||
                filePath.EndsWith(".fbx") ||
                filePath.EndsWith(".bfmdl"))
            {

                if (ModelFolder.TargtModel != null)
                    ModelFolder.TargtModel.AddModel(filePath);
                else
                    ModelFolder.ImportNewModel(filePath);

                return true;
            }
            return base.OnFileDrop(filePath);
        }

        public override void OnKeyDown(KeyEventInfo keyEventInfo)
        {
            if (keyEventInfo.IsKeyDown(InputSettings.INPUT.Scene.Delete)) {
                Renderer.RemoveSelected();
            }
            if (keyEventInfo.IsKeyDown(InputSettings.INPUT.Scene.SelectAll)) {
                foreach (BfresModelRender model in Renderer.Models) {
                    foreach (BfresMeshRender mesh in model.Meshes) {
                        mesh.IsSelected = true;
                    }
                }
            }
            if (keyEventInfo.IsKeyDown(InputSettings.INPUT.Scene.Hide))
            {
                foreach (BfresModelRender model in Renderer.Models)
                {
                    foreach (BfresMeshRender mesh in model.Meshes)
                    {
                        if (mesh.IsSelected)
                            mesh.UINode.IsChecked = !mesh.UINode.IsChecked;
                        if (keyEventInfo.KeyAlt)
                            mesh.UINode.IsChecked = true;
                    }
                }
            }
        }

        public void Save(Stream stream) {

            ModelFolder.OnSave();

            if (UpdateTransformedVertices)
            {
                //Update all meshes from new transform
                foreach (var model in ModelFolder.Models) {
                    foreach (FSHP mesh in model.Meshes) {
                        mesh.UpdateTransformedVertices(Renderer.Transform.TransformMatrix);
                    }
                }
                UpdateTransformedVertices = false;
            }

            TextureFolder.OnSave();
            AnimationsFolder.OnSave();

            ResFile.Save(stream);
        }

        public void Dispose() {
            DataCache.ModelCache.Remove(Renderer.Name);
        }

        public override List<MenuItemModel> GetViewMenuItems()
        {
            List<MenuItemModel> menus = new List<MenuItemModel>();

            menus.Add(new MenuItemModel($"      {IconManager.BONE_ICON}      Show Bones", () =>
            {
                Runtime.DisplayBones = !Runtime.DisplayBones;
                GlobalSettings.Current.Bones.Display = Runtime.DisplayBones;
                GlobalSettings.Current.Save();

                GLContext.ActiveContext.UpdateViewport = true;
            }, "", Runtime.DisplayBones));

            menus.Add(new MenuItemModel($"      {IconManager.PARTICLE_ICON}      Game Shaders", () =>
            {
                PluginConfig.UseGameShaders = !PluginConfig.UseGameShaders;
                PluginConfig.Instance.Save();

                Workspace.ActiveWorkspace.ReloadViewportMenu();
                GLContext.ActiveContext.UpdateViewport = true;
            }, "", PluginConfig.UseGameShaders));

            
            return menus;
        }

        public override void DrawViewportMenuBar()
        {
            //Target model for applying edits to
            string targetModel = ModelFolder.TargtModel != null ? ModelFolder.TargtModel.Name : "None";

            ImGui.PushItemWidth(200);
            if (ImGui.BeginCombo("##targetModel", $"Target: {targetModel}", ImGuiComboFlags.NoArrowButton))
            {
                foreach (var model in ModelFolder.Models)
                {
                    bool select = model == ModelFolder.TargtModel;
                    if (ImGui.Selectable(model.Name, select))
                        ModelFolder.TargtModel = model;

                    if (select)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();

            MapStudio.UI.ImGuiHelper.Tooltip("Determines what model to import meshes over during a .dae/.fbx drag/drop.");
        }
    }
}
