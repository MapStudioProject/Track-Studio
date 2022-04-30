using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Numerics;
using System.Linq;
using ImGuiNET;
using BfresLibrary;
using Toolbox.Core;
using Toolbox.Core.ViewModels;
using MapStudio.UI;

namespace CafeLibrary
{
    public class MaterialPresetWindow : UIFramework.Window
    {
        public EventHandler OnApplied;
        public EventHandler OnCancelled;

        public bool IsSwitch = false;

        public string GetSelectedPreset() => selectedMaterial.FilePath;
        public MaterialPreset GetSelectedPresetData() => selectedMaterial;

        public bool KeepTextures() => keepTextures;

        private bool keepTextures = true;

        private bool displayInfo = false;

        private MaterialPreset selectedMaterial;

        NodeBase GlobalPresets = new NodeBase();

        private bool loaded = false;
        private bool _isSwitch;
        public void LoadPresets(bool isSwitch)
        {
            _isSwitch = isSwitch;

            if (!Directory.Exists($"{Runtime.ExecutableDir}\\Presets\\Materials\\"))
                Directory.CreateDirectory($"{Runtime.ExecutableDir}\\Presets\\Materials\\");

            GlobalPresets = GetPresetsFromFolder($"{Runtime.ExecutableDir}\\Presets\\Materials\\", _isSwitch);
            loaded = true;
        }

        public void DownloadPresets()
        {
            try
            {
                UpdaterHelper.Setup("MapStudioProject", "MapStudio-Materials", "VersionMats.txt");

                var release = UpdaterHelper.TryGetLatest(Runtime.ExecutableDir, 0);
                if (release == null)
                    TinyFileDialog.MessageBoxInfoOk($"Build is up to date with the latest repo!");
                else
                {
                    int result = TinyFileDialog.MessageBoxInfoYesNo($"Found new release {release.Name}! Do you want to update?");
                    if (result == 1)
                    {
                        //Download
                        UpdaterHelper.DownloadRelease($"{Runtime.ExecutableDir}\\Presets\\Materials", release, 0).Wait();
                        GlobalPresets.Children.Clear();
                        selectedMaterial = null;

                        //Exit the tool and install via the updater
                        UpdaterHelper.Install($"{Runtime.ExecutableDir}\\Presets\\Materials");
                        //Reload presets
                        GlobalPresets = GetPresetsFromFolder($"{Runtime.ExecutableDir}\\Presets\\Materials\\", _isSwitch);
                    }
                }
            }
            catch (Exception ex)
            {
                DialogHandler.ShowException(ex);
            }
        }

        private NodeBase GetPresetsFromFolder(string folder, bool isSwitch)
        {
            NodeBase parent = new NodeBase(new DirectoryInfo(folder).Name);
            parent.Icon = IconManager.FOLDER_ICON.ToString();
            parent.IsExpanded = true;

            foreach (var dir in Directory.GetDirectories(folder))
                parent.AddChild(GetPresetsFromFolder(dir, isSwitch));

            foreach (var preset in Directory.GetFiles(folder))
            {
                if (preset.EndsWith(".zip"))
                {
                    try
                    {
                        var matPreset = new MaterialPreset(preset);
                        if (matPreset.Material.RenderState != null && isSwitch)
                            continue;
                        if (matPreset.Material.RenderState == null && !isSwitch)
                            continue;

                        parent.AddChild(matPreset);
                    }
                    catch
                    {

                    }
                }
            }
            return parent;
        }

        public override void Render()
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            var width = ImGui.GetWindowWidth();
            var height = ImGui.GetWindowHeight();

            if (ImGui.Button($"Download Presets", new Vector2(width, 25)))
                DownloadPresets();

            ImGui.Text("Select a material preset to assign!");

            ImGui.Checkbox("Keep Current Textures", ref keepTextures);
            ImGui.Checkbox("Show Info", ref displayInfo);

            ImGui.BeginColumns("material_preset_panel", displayInfo ? 2 : 1);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 1));
            RenderMaterialList();
            ImGui.PopStyleVar();

            if (displayInfo)
            {
                ImGui.NextColumn();

                if (selectedMaterial != null)
                    RenderMaterialInfo();

                ImGui.NextColumn();
            }

            ImGui.EndColumns();

            ImGui.SetCursorPosY(height - 34);

            if (ImGui.Button("Apply", new Vector2(width / 2, 30)) && selectedMaterial != null)
            {
                Apply();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(width / 2, 30)))
            {
                Cancel();
            }
            ImGui.PopStyleColor();
        }

        private void RenderMaterialList()
        {
            var width = ImGui.GetColumnWidth();
            var height = ImGui.GetWindowHeight();
            var posY = ImGui.GetCursorPosY();

            if (ImGui.CollapsingHeader("Global Materials", ImGuiTreeNodeFlags.DefaultOpen)) {

                if (ImGui.BeginChild("treeviewP", new Vector2(width - 30, height - posY  - 70)))
                {
                    DrawPresetTree(GlobalPresets);
                }
                ImGui.EndChild();
            }
        }

        private void Apply()
        {
            if (OnApplied == null)
                DialogHandler.ClosePopup(true);
            OnApplied?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            if (OnCancelled == null)
                DialogHandler.ClosePopup(false);
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }

        List<NodeBase> selected = new List<NodeBase>();

        private void DrawPresetTree(NodeBase node)
        {
            if (node is MaterialPreset && !((MaterialPreset)node).Display())
                return;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;
            flags |= ImGuiTreeNodeFlags.SpanFullWidth;

            if (node.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;
            else
            {
                flags |= ImGuiTreeNodeFlags.OpenOnDoubleClick;
                flags |= ImGuiTreeNodeFlags.OpenOnArrow;
            }
            if (node.IsExpanded)
            {
                //Flags for opening as default settings
                flags |= ImGuiTreeNodeFlags.DefaultOpen;
                //Make sure the "IsExpanded" can force the node to expand
                ImGui.SetNextItemOpen(true);
            }
            if (node.IsSelected)
                flags |= ImGuiTreeNodeFlags.Selected;

            //Improve tree spacing
            var spacing = ImGui.GetStyle().ItemSpacing;
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(spacing.X, 1));
            //Set the same header colors as hovered and active. This makes nav scrolling more seamless looking
            var active = ImGui.GetStyle().Colors[(int)ImGuiCol.Header];
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, active);
            ImGui.PushStyleColor(ImGuiCol.NavHighlight, new Vector4(0));

            ImGui.AlignTextToFramePadding();
            node.IsExpanded = ImGui.TreeNodeEx(node.ID, flags);

            ImGui.SameLine(); ImGuiHelper.IncrementCursorPosX(3);

            bool isToggleOpened = ImGui.IsItemToggledOpen();
            bool leftClicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
            bool rightClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
            bool nodeFocused = ImGui.IsItemFocused();

            if (node is MaterialPreset)
            {
                if ((MaterialPreset)node == selectedMaterial && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                {
                    Apply();
                }
            }

            ImGui.PopStyleColor(2);
            ImGui.PopStyleVar(1);

            void SelectNode()
            {
                //deselect previous
                foreach (var n in selected)
                    n.IsSelected = false;

                //select new
                selected.Clear();
                selected.Add(node);
                node.IsSelected = true;

                if (node is MaterialPreset)
                {
                    selectedMaterial = (MaterialPreset)node;

                    //Disable "keep textures" for presets with provided textures
                    //As the provided textures is what is recommended to use for the material preset.
                    //If the user wants to not use them then they can still uncheck the option.
                    if (selectedMaterial.HasTextures)
                        keepTextures = false;
                    else
                        keepTextures = true;
                }
                else
                    selectedMaterial = null;
            }

            if ((leftClicked || rightClicked) && !isToggleOpened) //Prevent selection change on toggle
                SelectNode();
            else if (nodeFocused && !isToggleOpened && !node.IsSelected)
                SelectNode();

            if (node.Icon?.Length == 1) //char icon
                IconManager.DrawIcon(node.Icon[0], Vector4.One);

            ImGui.SameLine();
            ImGuiHelper.IncrementCursorPosX(3);

            ImGui.Text(node.Header);

            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                    DrawPresetTree(child);

                ImGui.TreePop();
            }
        }

        private void RenderMaterialInfo()
        {
            ImGui.Text($"Material info:");

            var mat = selectedMaterial.Material;
            if (mat == null)
                return;

            if (ImGui.CollapsingHeader("Texture Map Info", ImGuiTreeNodeFlags.DefaultOpen)) {
                foreach (var sampler in mat.Samplers)
                {
                    if (TextureTypes.ContainsKey(sampler.Key))
                        ImGui.Text($"- {TextureTypes[sampler.Key]}:");
                }
            }
            if (ImGui.CollapsingHeader("Render Info", ImGuiTreeNodeFlags.DefaultOpen)) {
                if (mat.RenderState != null)
                    ImGui.Text($"RenderState: {mat.RenderState.FlagsMode}");
            }
            if (ImGui.CollapsingHeader("Settings", ImGuiTreeNodeFlags.DefaultOpen)) {
                ShowShaderOption(mat, "enable_fog", "Has Fog");
                ShowShaderOption(mat, "enable_dynamic_depth_shadow", "Display Shadows");
            }

            if (ImGui.CollapsingHeader("Files Included", ImGuiTreeNodeFlags.DefaultOpen)) {
                foreach (var file in selectedMaterial.FileAssets)
                    ImGui.BulletText(file);
            }
        }

        static void ShowShaderOption(Material material, string name, string label)
        {
            if (material.ShaderAssign.ShaderOptions.ContainsKey(name))
            {
                string value = material.ShaderAssign.ShaderOptions[name];
                ImGui.Text($"{label}: {(value == "0" ? "False" : "True")}");
            }
        }

        static Dictionary<string, string> TextureTypes = new Dictionary<string, string>()
        {
            { "_a0", "Albedo" },
            { "_n0", "Normal" },
            { "_s0", "Specular" },
            { "_e0", "Emissive"},
            { "_b0", "Baked Shadows" },
            { "_b1", "Baked Indirect Lighting" },
            { "_t0", "Transmission" },
        };

        public class MaterialPreset : NodeBase
        {
            public override string Header => Name;

            public string Name { get; set; }

            public bool HasTextures = false;

            public Material Material;

            public string FilePath;

            public List<string> FileAssets = new List<string>();

            public bool HasDynamicShadows = false;
            public bool HasTexSRT = false;

            public MaterialPreset(string filePath) 
            {
                FilePath = filePath;
                Name = Path.GetFileNameWithoutExtension(filePath);
                Icon = IconManager.FILE_ICON.ToString();

                var zipFile = ZipFile.OpenRead(filePath);
                foreach (var file in zipFile.Entries)
                    FileAssets.Add(file.Name);

                HasTextures = FileAssets.Any(x => x.EndsWith(".dds") || x.EndsWith(".bftex"));

                var presetFile = zipFile.GetEntry($"{Name}.json");
                using var sr = new StreamReader(presetFile.Open()); {
                    Material = BfresLibrary.TextConvert.MaterialConvert.FromJson(sr.ReadToEnd());
                }
                zipFile.Dispose();

                HasDynamicShadows = HasOptionValue("enable_dynamic_depth_shadow", "1");
            }
             
            public bool Display()
            {
                return true;
            }

            private bool HasOptionValue(string name, string value)
            {
                if (Material.ShaderAssign == null) return false;

                if (Material.ShaderAssign.ShaderOptions.ContainsKey(name))
                    return Material.ShaderAssign.ShaderOptions[name] == value;
                return false;
            }
        }
    }
}
