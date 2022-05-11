using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using KclLibrary;
using Toolbox.Core;

namespace TurboLibrary.CollisionEditor
{
    public class CollisionImporter
    {
        public KCLFile GetCollisionFile() => CollisionFile;

        CollisionPresetData Preset;
        CollisionMaterialSelector MaterialSelector = new CollisionMaterialSelector();

        private bool displayHex = false;
        //Determines to map attributes by meshes instead of materials
        private bool FilterByMeshes = false;
        //Selected material ID
        private ushort materialID = 0;
        //Selected materials
        List<string> selectedMaterials = new List<string>();

        private ObjModel ImportedModel;
        private KCLFile CollisionFile;

        //Results for the mapped material/mesh and attribute used
        private Dictionary<string, CollisionEntry> Results = new Dictionary<string, CollisionEntry>();

        //Keep track of error logs to render out (0 for normal, 1 for error)
        private List<Tuple<int, string>> logger = new List<Tuple<int, string>>();

        private bool IsBigEndian = true;
        private FileVersion Version = FileVersion.Version2;

        public CollisionImporter(bool isBigEndian = true, FileVersion version = FileVersion.Version2)
        {
            IsBigEndian = isBigEndian;
            Version = version;
            //Get the preset of all the collision material attributes.
            CollisionPresetData.LoadPresets(Directory.GetFiles(System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Presets","Collision")));
            Preset = CollisionPresetData.CollisionPresets.FirstOrDefault();
            MaterialSelector.AttributeCalculated += delegate
            {
                materialID = MaterialSelector.Attribute;
                foreach (var select in selectedMaterials)
                {
                    if (Results.ContainsKey(select))
                        Results[select].TypeID = materialID;
                }
            };
        }

        public void OpenObjectFile(IONET.Core.IOScene scene)
        {
            ImportedModel = new ObjModel(scene);
            UpdateMaterialList();
        }

        public void OpenObjectFile(string filePath)
        {
            ImportedModel = new ObjModel(filePath);
            UpdateMaterialList();
        }

        private void UpdateMaterialList()
        {
            Results.Clear();
            if (FilterByMeshes) {
                foreach (var mesh in ImportedModel.GetMeshNameList())
                    Results.Add(mesh, new CollisionEntry(mesh));
            }
            else {
                foreach (var mat in ImportedModel.GetMaterialNameList())
                    Results.Add(mat, new CollisionEntry(mat));
            }
           
            selectedMaterials.Clear();
            selectedMaterials.Add(Results.Keys.FirstOrDefault());
        }

        public void Render()
        {
            if (ImGui.Button(TranslationSource.GetText("APPLY")))
            {
                CollisionFile = ApplyDialog().Result;
                if (CollisionFile != null)
                    DialogHandler.ClosePopup(true);
            }

            if (logger.Count > 0)
            {
                foreach (var log in logger)
                {
                    if (log.Item1 == 1)
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), log.Item2);
                    else
                        ImGui.Text(log.Item2);
                }
                return;
            }

            bool updateList = false;

            ImGui.BeginColumns("col", 3);
            updateList |= ImGui.RadioButton(TranslationSource.GetText("MATERIAL_BY_MATERIALS"), !FilterByMeshes);
            ImGui.NextColumn();
            updateList |= ImGui.RadioButton(TranslationSource.GetText("MATERIAL_BY_MESHES"), FilterByMeshes);
            ImGui.NextColumn();
            ImGui.Checkbox(TranslationSource.GetText("DISPLAY_HEX"), ref displayHex);
            ImGui.NextColumn();
            ImGui.EndColumns();

            if (updateList) {
                FilterByMeshes = !FilterByMeshes;
                UpdateMaterialList();
            }

            int id = materialID;
            if (ImGui.InputInt(TranslationSource.GetText("ID"), ref id))
            {
                materialID = (ushort)MathF.Max(id, 0);
                foreach (var select in selectedMaterials)
                {
                    if (Results.ContainsKey(select))
                    {
                        Results[select].TypeID = materialID;
                        MaterialSelector.Update(materialID);
                    }
                }
            }

            MaterialSelector.Render();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            //Child is needed to keep dialog focused??
            ImGui.BeginChild("materialChild");

            var col = ImGui.GetStyle().Colors[(int)ImGuiCol.MenuBarBg];
            if (ImGui.BeginTable("listView", 5, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn(TranslationSource.GetText("MATERIAL"));
                ImGui.TableSetupColumn(TranslationSource.GetText("ID"));
                ImGui.TableSetupColumn(TranslationSource.GetText("ATTRIBUTE"));
                ImGui.TableSetupColumn(TranslationSource.GetText("MATERIAL"));
                ImGui.TableSetupColumn(TranslationSource.GetText("SPECIAL"));

                ImGui.TableHeadersRow();
                foreach (var material in Results)
                {
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    bool selected = ImGui.Selectable(material.Key, selectedMaterials.Contains(material.Key), ImGuiSelectableFlags.SpanAllColumns);
                    bool focused = ImGui.IsItemFocused();
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                    {
                        if (!selectedMaterials.Contains(material.Key))
                        {
                            if (!ImGui.GetIO().KeyCtrl && !ImGui.GetIO().KeyShift)
                                selectedMaterials.Clear();

                            selectedMaterials.Add(material.Key);
                        }
                        MaterialSelector.OpenPopup();
                    }

                    ImGui.TableNextColumn();
                    if (displayHex)
                        ImGui.Text($"0x{material.Value.TypeID.ToString("X4")}");
                    else
                        ImGui.Text(material.Value.TypeID.ToString());
                    ImGui.TableNextColumn();

                    string attributeName = MaterialSelector.GetAttributeName(material.Value.TypeID);
                    string materialName = MaterialSelector.GetAttributeMaterialName(material.Value.TypeID);

                    ImGui.Text(attributeName);
                    ImGui.TableNextColumn();

                    ImGui.Text(materialName);
                    ImGui.TableNextColumn();
                    ImGui.Text(MaterialSelector.GetSpecialTypeName(material.Value.TypeID));

                    if (selected || (focused && !selectedMaterials.Contains(material.Key)))
                    {
                        if (!ImGui.GetIO().KeyCtrl && !ImGui.GetIO().KeyShift)
                            selectedMaterials.Clear();

                        if (ImGui.GetIO().KeyShift)
                        {
                            bool selectRange = false;
                            foreach (var val in Results)
                            {
                                if (selectedMaterials.Contains(val.Key) || val.Key == material.Key)
                                {
                                    if (!selectRange)
                                        selectRange = true;
                                    else
                                        selectRange = false;
                                }
                                if (selectRange && !selectedMaterials.Contains(val.Key))
                                    selectedMaterials.Add(val.Key);
                            }
                        }

                        if (!selectedMaterials.Contains(material.Key))
                            selectedMaterials.Add(material.Key);

                        materialID = material.Value.TypeID;
                        MaterialSelector.Update(materialID);
                    }
                }
                ImGui.EndTable();
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();
        }

        private async Task<KCLFile> ApplyDialog()
        {
            logger.Clear();

            KCLFile kcl = null;

            DebugLogger.OnProgressUpdated += (s, e) =>
            {
                if (DebugLogger.IsCurrentError)
                    logger.Add(Tuple.Create(1, (string)s));
                else
                    logger.Add(Tuple.Create(0, (string)s));
            };

            await Task.Run(() =>
            {
                var settings = new CollisionImportSettings()
                {
                    SphereRadius = Preset.SphereRadius,
                    PrismThickness = Preset.PrismThickness,
                    PaddingMax = new Vector3(Preset.PaddingMax),
                    PaddingMin = new Vector3(Preset.PaddingMin),
                    MaxRootSize = Preset.MaxRootSize,
                    MinRootSize = Preset.MinRootSize,
                    MinCubeSize = Preset.MinCubeSize,
                    MaxTrianglesInCube = Preset.MaxTrianglesInCube,
                };

                foreach (var mesh in ImportedModel.Scene.Models[0].Meshes)
                {
                    foreach (var poly in mesh.Polygons)
                    {
                        if (!this.FilterByMeshes)
                        {
                            var mat = ImportedModel.Scene.Materials.FirstOrDefault(x => x.Name == poly.MaterialName);
                            if (mat != null)
                            {
                                string name = mat.Label == null ? mat.Name : mat.Label;
                                if (Results.ContainsKey(name))
                                    poly.Attribute = Results[name].TypeID;
                            }
                        }
                        else if (Results.ContainsKey(mesh.Name))
                            poly.Attribute = Results[mesh.Name].TypeID;
                    }
                }

                kcl = new KCLFile(ImportedModel.ToTriangles(), Version, IsBigEndian, settings);
            });
            return kcl;
        }

        class MaterialEntry
        {
            public string Name { get; set; }
            public int AttributeID { get; set; }
        }
    }
}
