using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;
using ImGuiNET;
using MapStudio.UI;
using IONET.Core;
using Toolbox.Core;

namespace CafeLibrary
{
    public class ModelImportDialog : UIFramework.Window
    {
        public override string Name => "Model Import Settings";

        ModelImportSettings Settings;
        FMDL Fmdl;

        List<int> selectedMeshIndices = new List<int>();
        bool openPresetWindow = false;

        MaterialPresetWindow PresetWindow = new MaterialPresetWindow();

        public void Setup(FMDL fmdl, IOScene scene, ModelImportSettings settings) {
            Settings = settings;
            PresetWindow.LoadPresets(fmdl.ResFile.IsPlatformSwitch);

            string DefaultPreset = "";
            bool isCourse = fmdl.ResFile.Name == "course_model.szs";
            bool isSwitch = fmdl.ResFile.IsPlatformSwitch;
            if (isCourse && !isSwitch && File.Exists($"{Runtime.ExecutableDir}\\Presets\\Materials\\MK8U\\Opaque\\Normal.zip"))
                DefaultPreset = $"{Runtime.ExecutableDir}\\Presets\\Materials\\MK8U\\Opaque\\Normal.zip";
            if (isCourse && isSwitch && File.Exists($"{Runtime.ExecutableDir}\\Presets\\Materials\\MK8D\\Opaque\\Normal.zip"))
                DefaultPreset = $"{Runtime.ExecutableDir}\\Presets\\Materials\\MK8D\\Opaque\\Normal.zip";

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
                    meshSettings.PresetName = $"Opaque\\{System.IO.Path.GetFileNameWithoutExtension(meshSettings.MaterialRawFile)}";

                //Select similar data
                if (fmdl.Meshes.Any(x => x.Name == mesh.Name))
                {
                    var fshp = (FSHP)fmdl.Meshes.FirstOrDefault(x => x.Name == mesh.Name);
                    //Keep matching materials for an import. User can readjust later
                    meshSettings.MaterialName = fshp.Material.Name;
                    foreach (var att in fshp.VertexBuffer.Attributes.Values)
                        meshSettings.AttributeLayout.Add(new ModelImportSettings.AttributeInfo(att.Name, att.BufferIndex));
                }

                if (!Settings.Materials.Contains(meshSettings.ImportedMaterial))
                    Settings.Materials.Add(meshSettings.ImportedMaterial);

                Settings.Meshes.Add(meshSettings);
            }
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

            if (ImguiCustomWidgets.BeginTab("importTab", "Model"))
            {
                if (ImGui.CollapsingHeader("Global Settings", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    DrawGlobalSettings();
                }
                if (ImGui.CollapsingHeader("Mesh Settings", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var wndsize = new Vector2(ImGui.GetWindowWidth() - 4, ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 37);
                    ImGui.BeginChild("##mesh_sett", wndsize);

                    var listsize = new Vector2(ImGui.GetColumnWidth() - 6, ImGui.GetWindowHeight() - 20);
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);
                    if (ImGui.BeginChild("##mesh_list", listsize))
                    {
                        DrawMeshList();
                    }
                    ImGui.EndChild();

                    ImGui.PopStyleColor();

                    ImGui.EndChild();
                }
                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("importTab", "Advanced"))
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
                        DrawMeshListAdvancedTab();
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

                ImGui.EndTabItem();
            }
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

        private void DrawGlobalSettings()
        {
            RenderMaterialSelector(Settings.MaterialPresetName, true);

            ImGui.BeginColumns("glbSettings", 2);
            ImGui.SetColumnWidth(0, 200);
            ImGui.Checkbox($"Flip UVs", ref Settings.FlipUVs);
            ImGui.NextColumn();
            ImGui.Checkbox($"Import Bones", ref Settings.ImportBones);
            ImGui.NextColumn();
            ImGui.Checkbox($"Enable Sub Mesh", ref Settings.EnableSubMesh);
            ImGui.NextColumn();
            ImGui.Checkbox($"Enable LODs", ref Settings.EnableLODs);
            ImGui.NextColumn();

            ImGui.EndColumns();

            if (ImGui.RadioButton($"Rotate None", !Settings.Rotate90 && !Settings.RotateNeg90))
            {
                Settings.Rotate90 = false;
                Settings.RotateNeg90 = false;
            }
            ImGui.SameLine();
            if (ImGui.RadioButton($"Rotate 90 Degrees", Settings.Rotate90 && !Settings.RotateNeg90))
            {
                Settings.Rotate90 = true;
                Settings.RotateNeg90 = false;
            }
            ImGui.SameLine();
            if (ImGui.RadioButton($"Rotate -90 Degrees", Settings.RotateNeg90 && !Settings.Rotate90))
            {
                Settings.RotateNeg90 = true;
                Settings.Rotate90 = false;
            }
        }

        private void DrawMeshListAdvancedTab()
        {
            ImGui.Columns(2);
            ImGuiHelper.BoldText("Meshes");
            ImGui.NextColumn();

            ImGuiHelper.BoldText("Materials");
            ImGui.NextColumn();

            for (int i = 0; i < Settings.Meshes.Count; i++)
            {
                bool select = selectedMeshIndices.Contains(i);
                if (ImGui.Selectable($"   {' '}   {Settings.Meshes[i].Name}", select))
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
                ImGui.Text($"{Settings.Meshes[i].ImportedMaterial}");
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        private void DrawMeshList()
        {
            ImGui.Columns(2);
            ImGuiHelper.BoldText("Meshes");
            ImGui.NextColumn();

            ImGuiHelper.BoldText("Materials Preset");
            ImGui.NextColumn();

            for (int i = 0; i < Settings.Meshes.Count; i++)
            {
                bool select = selectedMeshIndices.Contains(i);
                if (ImGui.Selectable($"   {' '}   {Settings.Meshes[i].Name}", select))
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
                if (ImGui.Button($"  {'\uf5fd'}   ##{Settings.Meshes[i].Name}matpreset"))
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
                            name = $"{preset.Parent.Header}\\{name}";

                        //Batch edit
                        foreach (var index in selectedMeshIndices)
                        {
                            Settings.Meshes[index].KeepTextures = PresetWindow.KeepTextures();
                            Settings.Meshes[index].PresetName = name;
                            Settings.Meshes[index].MaterialName = Settings.Meshes[index].ImportedMaterial;
                            Settings.Meshes[index].MaterialRawFile = presetPath;
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
                        foreach (var index in selectedMeshIndices)
                        {
                            Settings.Meshes[index].PresetName = name;
                            Settings.Meshes[index].MaterialName = Settings.Meshes[index].ImportedMaterial;
                            Settings.Meshes[index].MaterialRawFile = dlg.FilePath;
                        }
                    }
                }
                ImGui.SameLine();
                ImGui.Text($"{Settings.Meshes[i].PresetName}");
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
                        name = $"{preset.Parent.Header}\\{name}";

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
            if (ImGui.BeginCombo("Material", material))
            {
                /*   if (Settings.Materials.Count > 0)
                       ImGuiHelper.BoldText("DAE Materials");

                   foreach (var mat in Settings.Materials)
                   {
                       if (Fmdl.Model.Materials.ContainsKey(mat))
                           continue;

                       bool select = mat == material;
                       if (ImGui.Selectable(mat, select))
                       {
                           //Batch edit
                           if (isGlobal)
                           {
                               foreach (var mesh in Settings.Meshes)
                                   mesh.MaterialName = mat;
                           }
                           else
                           {
                               foreach (var index in selectedMeshIndices)
                                   Settings.Meshes[index].MaterialName = mat;
                           }
                       }
                       if (select)
                           ImGui.SetItemDefaultFocus();
                   }*/
                if (ImGui.Selectable("None", string.IsNullOrEmpty(material)))
                {
                    if (isGlobal)
                    {
                        foreach (var mesh in Settings.Meshes)
                            mesh.MaterialName = "";
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

            var mesh = Settings.Meshes[selectedMeshIndices.FirstOrDefault()];
            RenderMaterialSelector(mesh.MaterialName);

            uint total = 0;
            foreach (var msh in selectedMeshIndices)
                total += Settings.Meshes[msh].CalculateVertexBufferSize();

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

            ImGui.InputInt("Skin Count", ref mesh.SkinCount);
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
