using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;
using ImGuiNET;
using MapStudio.UI;
using IONET.Core;
using Toolbox.Core;
using BfresLibrary;

namespace CafeLibrary
{
    public class ModelImportDialog : UIFramework.Window
    {
        ModelImportSettings Settings;
        FMDL Fmdl;

        List<int> selectedMeshIndices = new List<int>();
        bool openPresetWindow = false;

        MaterialPresetWindow PresetWindow = new MaterialPresetWindow();

        public ModelImportDialog() : base("Model Import Settings", new Vector2(200, 200))
        {

        }

        public void OnApply(List<Material> materials)
        {
            foreach (var mesh in this.Settings.Meshes)
            {
                //Map original by default
                var mat = materials.FirstOrDefault(x => x.Name == mesh.MaterialName);
                //Map to imported instance if needed
                if (mesh.MaterialInstance != null)
                    mat = mesh.MaterialInstance;

                if (mat != null) {
                    //Use combined UVs if used by the material
                    mesh.CombineUVs = mat.ShaderAssign != null &&
                                      mat.ShaderAssign.AttribAssigns.ContainsValue("_g3d_02_u0_u1");
                }
            }
        }

        public void Setup(FMDL fmdl, IOScene scene, ModelImportSettings settings)
        {
            Settings = settings;
            PresetWindow.LoadPresets(fmdl.ResFile.IsPlatformSwitch);

            string DefaultPreset = "";
            bool isCourse = fmdl.ResFile.Name == "course_model.szs";
            bool isSwitch = fmdl.ResFile.IsPlatformSwitch;
            if (isCourse && !isSwitch && File.Exists(Path.Combine(Runtime.ExecutableDir, "Presets", "Materials", "MK8U", "Opaque", "Normal.zip")))
                DefaultPreset = Path.Combine(Runtime.ExecutableDir, "Presets", "Materials", "MK8U", "Opaque", "Normal.zip");
            if (isCourse && isSwitch && File.Exists(Path.Combine(Runtime.ExecutableDir, "Presets", "Materials", "MK8D", "Opaque", "Normal.zip")))
                DefaultPreset = Path.Combine(Runtime.ExecutableDir, "Presets", "Materials", "MK8D", "Opaque", "Normal.zip");

            Fmdl = fmdl;

            Settings.Meshes.Clear();
            foreach (var mesh in scene.Models[0].Meshes.OrderBy(x => x.Name))
            {
                if (mesh.Vertices.Count == 0)
                    continue;

                var meshSettings = new ModelImportSettings.MeshSettings();
                meshSettings.MeshData = mesh;
                meshSettings.Name = mesh.Name;
                meshSettings.MaterialName = "";
                meshSettings.SkinCount = mesh.Vertices.Max(x => x.Envelope.Weights.Count);
                meshSettings.Normal.Enable = mesh.HasNormals;
                meshSettings.UVs.Enable = mesh.HasUVSet(0);
                meshSettings.UVLayerCount = (uint)MathF.Max(mesh.Vertices.Max(x => x.UVs.Count), 1);
                //Tangents/bitangents auto generate if has UVs
                meshSettings.Tangent.Enable = meshSettings.UVs.Enable;
                meshSettings.Bitangent.Enable = meshSettings.UVs.Enable;
                meshSettings.Colors.Enable = mesh.HasColorSet(0);
                meshSettings.BoneIndices.Enable = mesh.HasEnvelopes();
                meshSettings.BoneWeights.Enable = mesh.HasEnvelopes();
                meshSettings.ImportedMaterial = mesh.Polygons[0].MaterialName;

                meshSettings.MaterialRawFile = DefaultPreset;
                if (!string.IsNullOrEmpty(meshSettings.MaterialRawFile))
                    meshSettings.PresetName = Path.Combine("Opaque", System.IO.Path.GetFileNameWithoutExtension(meshSettings.MaterialRawFile));

                //Select similar data
                if (fmdl.Meshes.Any(x => x.Name == mesh.Name))
                {
                    var fshp = (FSHP)fmdl.Meshes.FirstOrDefault(x => x.Name == mesh.Name);
                    //Keep matching materials for an import. User can re adjust later
                    meshSettings.MaterialName = fshp.Material.Name;
                    foreach (var att in fshp.VertexBuffer.Attributes.Values)
                        meshSettings.AttributeLayout.Add(new ModelImportSettings.AttributeInfo(att.Name, att.BufferIndex));

                    meshSettings.BoneIndex = (ushort)fshp.BoneIndex;
                }

                if (!Settings.Materials.Contains(meshSettings.ImportedMaterial))
                    Settings.Materials.Add(meshSettings.ImportedMaterial);

                Settings.Meshes.Add(meshSettings);
            }

            Settings.Position.Enable = Settings.Meshes.Any(x => x.Position.Enable);
            Settings.Normal.Enable = Settings.Meshes.Any(x => x.Normal.Enable);
            Settings.UVs.Enable = Settings.Meshes.Any(x => x.UVs.Enable);
            Settings.Tangent.Enable = Settings.Meshes.Any(x => x.Tangent.Enable);
            Settings.Bitangent.Enable = Settings.Meshes.Any(x => x.Bitangent.Enable);
            Settings.BoneIndices.Enable = Settings.Meshes.Any(x => x.BoneIndices.Enable);
            Settings.Colors.Enable = Settings.Meshes.Any(x => x.Colors.Enable);

            selectedMeshIndices.Add(0);
        }

        public override void Render()
        {
            if (openPresetWindow)
            {
                ImGui.OpenPopup("materialPresetWindow");
                openPresetWindow = false;
            }

            if (ImGui.BeginPopup("materialPresetWindow"))
            {
                if (ImGui.BeginChild("presetChild", new Vector2(500, 600)))
                {
                    PresetWindow.Render();
                }
                ImGui.EndChild();
                ImGui.EndPopup();
            }

            ImGui.BeginTabBar("importTab");

            if (ImguiCustomWidgets.BeginTab("importTab", "Model Settings"))
            {
                DrawGlobalSettings();

                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("importTab", "Mesh Settings"))
            {
                DrawMeshTab();

                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("importTab", "Advanced Settings"))
            {
                DrawAdvancedTab();

                ImGui.EndTabItem();
            }
            /*   if (ImguiCustomWidgets.BeginTab("importTab", "Inject Settings"))
               {
                   DrawMeshTab();

                   ImGui.EndTabItem();
               }*/


            ImGui.EndTabBar();

            var pos = ImGui.GetWindowHeight() - 26;
            ImGui.SetCursorPosY(pos);

            var size = new Vector2(ImGui.GetWindowWidth() / 2, 23);
            if (ImGui.Button(TranslationSource.GetText("CANCEL"), size))
                DialogHandler.ClosePopup(false);

            ImGui.SameLine();
            if (ImGui.Button(TranslationSource.GetText("OK"), size))
                DialogHandler.ClosePopup(true);
        }

        private void DrawMeshTab()
        {
            if (ImGui.CollapsingHeader("Mesh Settings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var wndsize = new Vector2(ImGui.GetWindowWidth() - 4, ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 37);
                ImGui.BeginChild("##mesh_sett", wndsize);

                ImGui.Columns(2);

                var listsize = new Vector2(ImGui.GetColumnWidth() - 6, ImGui.GetWindowHeight() - 20);
                ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);
                if (ImGui.BeginChild("##mesh_list", listsize))
                {
                    DrawMeshListTab();
                }
                ImGui.EndChild();

                ImGui.PopStyleColor();
                ImGui.NextColumn();

                var infosize = new Vector2(ImGui.GetColumnWidth() - 6, ImGui.GetWindowHeight() - 20);
                ImGuiHelper.BoldText("Mesh Info");

                if (ImGui.BeginChild("##mesh_data", infosize))
                {
                    DrawMeshProperties();
                }
                ImGui.EndChild();

                ImGui.NextColumn();

                ImGui.Columns(1);
                ImGui.EndChild();
            }
        }

        private void DrawAdvancedTab()
        {
            ImGui.BeginColumns("formatSetting", 2);

            //Global settings where each format is globally applied to all meshes
            DrawAttribute("Positions", this.Settings.Position, () => {
                Settings.Meshes.ForEach(x => x.Position.Format = Settings.Position.Format);
            });
            DrawAttribute("Normals", this.Settings.Normal, () => {
                Settings.Meshes.ForEach(x => x.Normal.Format = Settings.Normal.Format);
            });
            DrawAttribute("UVS", this.Settings.UVs, () => {
                Settings.Meshes.ForEach(x => x.UVs.Format = Settings.UVs.Format);
            });
            DrawAttribute("Vertex Colors", this.Settings.Colors, () => {
                Settings.Meshes.ForEach(x => x.Colors.Format = Settings.Colors.Format);
            });

            ImGui.NextColumn();

            DrawAttribute("Tangents", this.Settings.Tangent, () => {
                Settings.Meshes.ForEach(x => x.Tangent.Format = Settings.Tangent.Format);
            });
            DrawAttribute("Bitangents", this.Settings.Bitangent,() => {
                Settings.Meshes.ForEach(x => x.Bitangent.Format = Settings.Bitangent.Format);
            });
            DrawAttribute("Bone Indices", this.Settings.BoneIndices,  () => {
                Settings.Meshes.ForEach(x => x.BoneIndices.Format = Settings.BoneIndices.Format);
            });
            DrawAttribute("Bone Weights", this.Settings.BoneWeights, () => {
                Settings.Meshes.ForEach(x => x.BoneWeights.Format = Settings.BoneWeights.Format);
            });

            ImGui.EndColumns();
        }

        private void DrawAttribute(string attribute, ModelImportSettings.AttributeSettings att, Action update)
        {
            ImGuiHelper.BoldText(attribute);

            ImGui.Text("Format:     ");
            ImGui.SameLine();

            if (ImGui.BeginCombo($"##formatList{attribute}", ModelImportSettings.GetFormatName(att.Format)))
            {
                foreach (var format in att.FormatList)
                {
                    string formatText = ModelImportSettings.GetFormatName(format);
                    bool select = format == att.Format;
                    if (ImGui.Selectable(formatText, select))
                    {
                        att.Format = format;
                        update.Invoke();
                    }

                    if (select)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }
        }

        private void DrawGlobalSettings()
        {
            GamePresetDropdown();

            ImGui.BeginColumns("glbSettings", 2);
            ImGui.Checkbox($"Flip UVs", ref Settings.FlipUVs);
            ImGui.Checkbox($"Use DAE Bones", ref Settings.ImportBones);

            if (ImGui.RadioButton($"Rotate None", !Settings.Rotate90 && !Settings.RotateNeg90))
            {
                Settings.Rotate90 = false;
                Settings.RotateNeg90 = false;
            }
            if (ImGui.RadioButton($"Rotate 90 Degrees", Settings.Rotate90 && !Settings.RotateNeg90))
            {
                Settings.Rotate90 = true;
                Settings.RotateNeg90 = false;
            }
            if (ImGui.RadioButton($"Rotate -90 Degrees", Settings.RotateNeg90 && !Settings.Rotate90))
            {
                Settings.RotateNeg90 = true;
                Settings.Rotate90 = false;
            }

            ImGui.Checkbox($"Recalculate Normals", ref Settings.RecalculateNormals);
            ImGui.Checkbox($"Override Vertex Colors", ref Settings.OverrideVertexColors);
            if (Settings.OverrideVertexColors)
            {
                ImGui.SameLine();
                ImGui.ColorEdit4("##ColorOverride", ref Settings.ColorOverride, ImGuiColorEditFlags.NoInputs);
            }

            ImGui.Checkbox($"Enable Sub Mesh", ref Settings.EnableSubMesh);

            bool useMaterial = !Settings.KeepOrginalMaterialsOnReplace;
            if (ImGui.Checkbox("Use Custom Material", ref useMaterial))
                Settings.KeepOrginalMaterialsOnReplace = !useMaterial;

            if (useMaterial)
            {
                if (string.IsNullOrEmpty(Settings.MaterialPresetName))
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.5f, 0, 0, 1f));
                    RenderMaterialSelector(Settings.MaterialPresetName, true);
                    ImGui.PopStyleColor();
                }
                else
                {
                    RenderMaterialSelector(Settings.MaterialPresetName, true);
                }
            }

            ImGui.NextColumn();

            //Global attribute settings that apply to all meshes at once
            if (ImGui.Checkbox($"Enable Positions", ref Settings.Position.Enable))
                Settings.Meshes.ForEach(x => x.Position.Enable = Settings.Position.Enable);

            if (ImGui.Checkbox($"Enable Normals", ref Settings.Normal.Enable))
                Settings.Meshes.ForEach(x => x.Normal.Enable = Settings.Normal.Enable);

            if (ImGui.Checkbox($"Enable UVs", ref Settings.UVs.Enable))
                Settings.Meshes.ForEach(x => x.UVs.Enable = Settings.UVs.Enable);

            if (ImGui.Checkbox($"Enable Vertex Colors", ref Settings.Colors.Enable))
                Settings.Meshes.ForEach(x => x.Colors.Enable = Settings.Colors.Enable);

            if (ImGui.Checkbox($"Enable Tangents", ref Settings.Tangent.Enable))
                Settings.Meshes.ForEach(x => x.Tangent.Enable = Settings.Tangent.Enable);

            if (ImGui.Checkbox($"Enable Bitangents", ref Settings.Bitangent.Enable))
                Settings.Meshes.ForEach(x => x.Bitangent.Enable = Settings.Bitangent.Enable);

            if (ImGui.Checkbox($"Enable Indices/Weights", ref Settings.BoneIndices.Enable))
            {
                foreach (var mesh in Settings.Meshes)
                {
                    mesh.BoneWeights.Enable = Settings.BoneIndices.Enable;
                    mesh.BoneIndices.Enable = Settings.BoneIndices.Enable;
                }
            }
            ImGui.Checkbox($"Reset UV Params", ref Settings.ResetUVParams);
            ImGui.Checkbox($"Reset Color Params", ref Settings.ReseColorParams);

            ImGui.Checkbox($"Enable LODs", ref Settings.EnableLODs);

            if (Settings.EnableLODs)
            {
                ImGui.InputInt("LOD Count", ref Settings.LODCount, 1);
            }

            ImGui.Checkbox($"Enable Sub Meshes (Experimental)", ref Settings.EnableSubMesh);
            
            ImGui.EndColumns();
        }

        private void GamePresetDropdown()
        {
            ImGui.BeginColumns("presetSetting", 2);

            ImGui.SetColumnWidth(0, 150);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Game Preset:");
            ImGui.NextColumn();

            if (ImGui.BeginCombo("##GamePreset", "Default"))
            {
                ImGui.EndCombo();
            }

            ImGui.NextColumn();
            ImGui.EndColumns();
        }

        private void DrawMeshListTab()
        {
            ImGui.Columns(2);
            ImGuiHelper.BoldText("Meshes");
            ImGui.NextColumn();

            ImGuiHelper.BoldText("Materials");
            ImGui.NextColumn();

            for (int i = 0; i < Settings.Meshes.Count; i++)
            {
                bool select = selectedMeshIndices.Contains(i);
                if (ImGui.Selectable($"   {' '}   {Settings.Meshes[i].Name}", select, ImGuiSelectableFlags.SpanAllColumns))
                {
                    if (!ImGui.GetIO().KeyShift)
                        selectedMeshIndices.Clear();

                    selectedMeshIndices.Add(i);

                    //Selection range
                    if (ImGui.GetIO().KeyShift)
                    {
                        var lowIndex = selectedMeshIndices.Min();
                        for (int j = lowIndex; j < i; j++)
                        {
                            if (!selectedMeshIndices.Contains(j))
                                selectedMeshIndices.Add(j);
                        }
                    }
                }
                if (ImGui.IsItemFocused() && !select)
                {
                    if (!ImGui.GetIO().KeyShift)
                        selectedMeshIndices.Clear();

                    selectedMeshIndices.Add(i);
                }

                ImGui.NextColumn();
                if (!string.IsNullOrEmpty(Settings.Meshes[i].PresetName))
                    ImGui.Text($"{Settings.Meshes[i].PresetName}");
                else
                    ImGui.Text(Settings.Meshes[i].MaterialName);
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        private void RenderMaterialSelector(string material, bool isGlobal = false)
        {
            //Preset selector
            if (ImGui.Button($"  {'\uf5fd'}   "))
            {
                //reload preset folder
                PresetWindow.OnCancelled = null;
                PresetWindow.OnCancelled += delegate
                {
                    ImGui.CloseCurrentPopup();
                };
                PresetWindow.OnApplied = null;
                PresetWindow.OnApplied += delegate
                {
                    ImGui.CloseCurrentPopup();

                    var preset = PresetWindow.GetSelectedPresetData();
                    var presetPath = preset.FilePath;
                    var name = preset.Name;
                    //Show atleast one level of the tree to see the category the preset is inside
                    if (preset.Parent != null)
                        name = Path.Combine(preset.Parent.Header, name);

                    //Batch edit
                    if (isGlobal)
                    {
                        foreach (var mesh in Settings.Meshes)
                        {
                            mesh.KeepTextures = PresetWindow.KeepTextures();
                            mesh.PresetName = name;
                            mesh.MaterialName = mesh.ImportedMaterial;
                            mesh.MaterialRawFile = presetPath;
                        }
                        Settings.MaterialPresetName = name;
                    }
                    else
                    {
                        foreach (var index in selectedMeshIndices)
                        {
                            Settings.Meshes[index].KeepTextures = PresetWindow.KeepTextures();
                            Settings.Meshes[index].PresetName = name;
                            Settings.Meshes[index].MaterialName = Settings.Meshes[index].ImportedMaterial;
                            Settings.Meshes[index].MaterialRawFile = presetPath;
                        }
                    }
                };
                openPresetWindow = true;
            }
            ImGui.SameLine();
            //External file selector
            if (ImGui.Button($"  {'\uf15b'}   "))
            {
                var dlg = new ImguiFileDialog();
                dlg.SaveDialog = false;
                dlg.AddFilter(".bfmat", ".bfmat");
                dlg.AddFilter(".json", ".json");
                dlg.AddFilter(".zip", ".zip");

                if (dlg.ShowDialog())
                {
                    var name = System.IO.Path.GetFileNameWithoutExtension(dlg.FilePath);

                    if (isGlobal)
                    {
                        foreach (var mesh in Settings.Meshes)
                        {
                            mesh.PresetName = name;
                            mesh.MaterialName = mesh.ImportedMaterial;
                            mesh.MaterialRawFile = dlg.FilePath;
                        }
                        Settings.MaterialPresetName = name;
                    }
                    else
                    {
                        foreach (var index in selectedMeshIndices)
                        {
                            Settings.Meshes[index].PresetName = name;
                            Settings.Meshes[index].MaterialName = Settings.Meshes[index].ImportedMaterial;
                            Settings.Meshes[index].MaterialRawFile = dlg.FilePath;
                        }
                    }
                }
            }
            ImGui.SameLine();

            if (ImGui.BeginCombo("##Material", string.IsNullOrEmpty(material) ? "(Select Material)" : material))
            {
                if (ImGui.Selectable("None", string.IsNullOrEmpty(material)))
                {
                    if (isGlobal)
                    {
                        foreach (var mesh in Settings.Meshes)
                            mesh.MaterialName = "";

                        Settings.MaterialPresetName = "";
                    }
                    else
                    {
                        foreach (var index in selectedMeshIndices)
                            Settings.Meshes[index].MaterialName = "";
                    }
                    material = "";
                }
                if (string.IsNullOrEmpty(material))
                    ImGui.SetItemDefaultFocus();

                ImGuiHelper.BoldText("BFRES Materials");

                //Go through all existing materials to select
                foreach (var mat in Fmdl.Materials)
                {
                    bool select = mat.Name == material;
                    if (ImGui.Selectable(mat.Name, select))
                    {
                        //Batch edit
                        if (isGlobal)
                        {
                            foreach (var mesh in Settings.Meshes)
                            {
                                mesh.MaterialName = mat.Name;
                                mesh.MaterialRawFile = "";
                                mesh.PresetName = "";
                            }
                            Settings.MaterialPresetName = mat.Name;
                        }
                        else
                        {
                            foreach (var index in selectedMeshIndices)
                            {
                                Settings.Meshes[index].MaterialName = mat.Name;
                                Settings.Meshes[index].PresetName = "";
                                Settings.Meshes[index].MaterialRawFile = "";
                            }
                        }
                    }
                    if (select)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
        }

        private void DrawMeshProperties()
        {
            if (selectedMeshIndices.Count == 0)
                return;

            ImGui.Columns(2);
            ImGui.Text("Material: ");
            ImGui.NextColumn();

            var mesh = Settings.Meshes[selectedMeshIndices.FirstOrDefault()];

            ImGui.PushItemWidth(ImGui.GetColumnWidth(1) - 15);
            RenderMaterialSelector(mesh.MaterialName);

            ImGui.NextColumn();

            uint total = 0;
            foreach (var msh in selectedMeshIndices)
                total += Settings.Meshes[msh].CalculateVertexBufferSize();

            ImGui.Text("Skin Count: ");
            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.GetColumnWidth(1) - 15);
            if (ImGui.InputInt("##Skin Count", ref mesh.SkinCount))
            {
                foreach (var msh in selectedMeshIndices)
                    Settings.Meshes[msh].SkinCount = mesh.SkinCount;
            }
            ImGui.NextColumn();

            ImGui.Columns(1);

            ImGui.Checkbox("Custom Attribute Settings", ref mesh.UseCustomAttributeSettings);

            if (mesh.UseCustomAttributeSettings)
            {
                ImGuiHelper.BoldText($"Memory Usage (Vertices): {Toolbox.Core.STMath.GetFileSize(total)}");

                ImGui.BeginColumns("meshInfoC", 2);

                AttributeSettings("Positions", mesh.Position);
                AttributeSettings("Normals", mesh.Normal);
                AttributeSettings("UVs", mesh.UVs);
                AttributeSettings("Vertex Colors", mesh.Colors);
                AttributeSettings("Tangent", mesh.Tangent);
                AttributeSettings("Bitangent", mesh.Bitangent);
                AttributeSettings("Bone Indices", mesh.BoneIndices);
                AttributeSettings("Bone Weights", mesh.BoneWeights);

                ImGui.EndColumns();
            }
        }

        private void AttributeSettings(string attribute, ModelImportSettings.AttributeSettings settings)
        {
            ImGui.Checkbox($"##enable{attribute}", ref settings.Enable);
            ImGui.SetColumnWidth(0, ImGui.GetItemRectSize().X + 10);

            ImGui.NextColumn();

            if (ImGui.CollapsingHeader($"{attribute}"))
            {
                if (ImGui.BeginCombo($"Format:##formatList{attribute}", ModelImportSettings.GetFormatName(settings.Format)))
                {
                    foreach (var format in settings.FormatList)
                    {
                        string formatText = ModelImportSettings.GetFormatName(format);
                        bool select = format == settings.Format;
                        if (ImGui.Selectable(formatText, select))
                            settings.Format = format;

                        if (select)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndCombo();
                }
            }
            ImGui.NextColumn();
        }
    }
}