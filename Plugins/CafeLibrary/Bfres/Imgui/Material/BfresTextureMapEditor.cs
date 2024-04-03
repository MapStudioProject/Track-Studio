using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Toolbox.Core;
using BfresLibrary;
using BfresLibrary.GX2;
using System.Numerics;
using MapStudio.UI;
using CafeLibrary.Rendering;

namespace CafeLibrary
{
    public class BfresTextureMapEditor
    {
        static List<int> SelectedIndices = new List<int>();

        static bool dialogOpened = false;

        public static void Reset()
        {
            SelectedIndices.Clear();
        }

        public static void Render(FMAT material, UVViewport UVViewport, bool onLoad)
        {
            float width = ImGui.GetWindowWidth();
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            if (ImGui.BeginChild("TEXTURE_MAP_LIST", new System.Numerics.Vector2(width, 100)))
            {
                ImGui.BeginColumns("textureList", 3);
                ImGuiHelper.BoldText("Name");
                ImGui.NextColumn();
                ImGuiHelper.BoldText("Sampler");
                ImGui.NextColumn();
                ImGuiHelper.BoldText("Shader Sampler");
                ImGui.NextColumn();
                ImGui.EndColumns();

                int index = 0;
                foreach (var texMap in material.TextureMaps)
                {
                    bool animated = material.AnimatedSamplers.ContainsKey(material.Samplers[index]);
                    if (animated)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0.5f, 0, 1));
                        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                    }

                    ImGui.BeginColumns("textureList", 3);

                    string name = texMap.Name;
                    if (material.AnimatedSamplers.ContainsKey(material.Samplers[index]))
                        name = material.AnimatedSamplers[material.Samplers[index]];

                    var tex = SearchTextureInstance(name);
                    if (tex != null)
                        IconManager.DrawTexture(name, tex);
                    else
                        IconManager.DrawIcon("TEXTURE");

                    ImGui.SameLine();

                    if (ImGui.Selectable($"{name}##texmap{index}", SelectedIndices.Contains(index), ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SelectedIndices.Clear();
                        SelectedIndices.Add(index);
                    }
                    if (ImGui.IsItemFocused() && !SelectedIndices.Contains(index))
                    {
                        SelectedIndices.Clear();
                        SelectedIndices.Add(index);
                    }

                    if (animated)
                    {
                        ImGui.PopStyleVar();
                        ImGui.PopStyleColor();
                    }

                    if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                    {
                        TextureSelectionDialog.Init();
                        dialogOpened = true;
                    }
                    if (SelectedIndices.Contains(index))
                    {
                        if (ImGui.BeginPopupContextItem("textureMenu", ImGuiPopupFlags.MouseButtonRight))
                        {
                            ImGui.AlignTextToFramePadding();
                            if (ImGui.Selectable("Change Texture"))
                            {
                                TextureSelectionDialog.Init();
                                dialogOpened = true;
                            }

                            ImGui.AlignTextToFramePadding();
                            if (ImGui.Selectable("Export Texture Data"))
                                material.ExportTextureMapData(texMap.Name);

                            ImGui.AlignTextToFramePadding();
                            if (ImGui.Selectable("Replace Texture Data"))
                                material.EditTextureMapData(texMap.Name);

                            ImGui.AlignTextToFramePadding();
                            if (ImGui.Selectable("Select Texture Data"))
                                material.SelectTextureMapData(texMap.Name);

                            ImGui.AlignTextToFramePadding();
                            if (ImGui.Selectable("Insert Key Frame"))
                                material.InsertTextureKey(material.Material.Samplers[index].Name, texMap.Name);

                            ImGui.EndPopup();
                        }
                    }

                    string fragSampler = material.Samplers[index];
                    string sampler = texMap.Sampler;
                    if (TextureHint.ContainsKey(fragSampler))
                        sampler = $"{sampler} ({TextureHint[fragSampler]})";

                    ImGui.NextColumn();
                    ImGui.Text(sampler);
                    ImGui.NextColumn();
                    ImGui.Text(fragSampler);
                    ImGui.NextColumn();

                    ImGui.EndColumns();

                    index++;
                }
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();

            if (material.TextureMaps.Count == 0)
                return;

            //Make sure there is atleast 1 selection used
            if (SelectedIndices.Count == 0)
                SelectedIndices.Add(0);

            if (ImGui.BeginChild("SAMPLER_DATA"))
            {
                SelectSampler(material, UVViewport, SelectedIndices.FirstOrDefault(), onLoad);
            }
            ImGui.EndChild();
        }

        static Dictionary<string, string> TextureHint = new Dictionary<string, string>()
        {
            { "_a0", "Diffuse Map" },
            { "_n0", "Normal Map" },
            { "_s0", "Specular Map" },
            { "_e0", "Emissive Map" },
            { "_t0", "Transmission Map" },
            { "_b0", "Shadow Map" },
            { "_b1", "Light Map" },
        };

        static void OpenTextureDialog(FMAT material, int index)
        {
            var texMap = material.TextureMaps[index];

            TextureSelectionDialog.Textures = material.GetTextures();
            if (TextureSelectionDialog.Render(texMap.Name, ref dialogOpened))
            {
                var input = TextureSelectionDialog.OutputName;
                texMap.Name = input;
                material.BatchTextureName(input, material.Material.Samplers[index].Name);
                material.ReloadTextureMap(index);
                material.OnTextureUpdated(material.Material.Samplers[index].Name, texMap.Name, true);
            }
        }

        static float uvWindowHeight = 150;

        static void SelectSampler(FMAT material, UVViewport UVViewport, int index, bool onLoad)
        {
            var materialData = material.Material;

            var sampler = materialData.Samplers[index].TexSampler;
            var texMapSel = material.TextureMaps[index];

            //Texture map info
            if (ImGui.CollapsingHeader("Texture Info", ImGuiTreeNodeFlags.DefaultOpen)) {
                if (ImGui.Button($"   {'\uf302'}   ")) {
                    dialogOpened = true;
                    TextureSelectionDialog.Init();
                }
                ImGui.SameLine();

                string name = texMapSel.Name;
                if (ImGui.InputText("Name", ref name, 200)) {
                    texMapSel.Name = name;
                    material.BatchTextureName(name, materialData.Samplers[index].Name);
                    material.ReloadTextureMap(index);
                }
            } 

            if (dialogOpened) {
                OpenTextureDialog(material, index);
            }

            var width = ImGui.GetWindowWidth();
            var size = ImGui.GetWindowSize();

            //A UV preview window drawn using opengl
            if (ImGui.CollapsingHeader("Preview", ImGuiTreeNodeFlags.DefaultOpen)) {

                if (ImGui.BeginChild("uv_viewport1", new Vector2(width, uvWindowHeight)))
                {
                    var pos = ImGui.GetCursorScreenPos();

                    if (onLoad)
                    {
                        var meshes = material.GetMappedMeshes();

                        UVViewport.ActiveObjects.Clear();
                        foreach (FSHP mesh in meshes)
                            UVViewport.ActiveObjects.Add(mesh);
                        UVViewport.UpdateVertexBuffer = true;
                    }

                    UVViewport.ActiveTextureMap = material.TextureMaps[index];
                    UVViewport.Render((int)width, (int)uvWindowHeight);

                    ImGui.SetCursorScreenPos(pos);
                    if (ImGui.Checkbox("Show UVs", ref UVViewport.DisplayUVs))
                    {

                    }
                }
                ImGui.EndChild();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.Separator]);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGui.GetStyle().Colors[(int)ImGuiCol.SeparatorHovered]);
                ImGui.Button("##hsplitter", new Vector2(-1, 2));
                if (ImGui.IsItemActive())
                {
                    var deltaY = -ImGui.GetIO().MouseDelta.Y;
                    if (uvWindowHeight - deltaY < size.Y - 22 && uvWindowHeight - deltaY > 22)
                        uvWindowHeight -= deltaY;
                }

                ImGui.PopStyleColor(2);
            }

            if (ImGui.BeginChild("sampler_properties"))
            {
                bool updated = LoadProperties(sampler);
                if(updated)
                    material.ReloadTextureMap(index);
            }
            ImGui.EndChild();
        }

        static bool LoadProperties(TexSampler sampler) {
            var flags = ImGuiTreeNodeFlags.DefaultOpen;
            bool updated = false;
            if (ImGui.CollapsingHeader("Wrap Mode", flags))
            {
                updated |= ImGuiHelper.ComboFromEnum<GX2TexClamp>("Wrap X", sampler, "ClampX");
                updated |= ImGuiHelper.ComboFromEnum<GX2TexClamp>("Wrap Y", sampler, "ClampY");
                updated |= ImGuiHelper.ComboFromEnum<GX2TexClamp>("Wrap Z", sampler, "ClampZ");
            }
            if (ImGui.CollapsingHeader("Filter", flags))
            {
                updated |= ImGuiHelper.ComboFromEnum<GX2TexXYFilterType>("Mag Filter", sampler, "MagFilter");
                updated |= ImGuiHelper.ComboFromEnum<GX2TexXYFilterType>("Min Filter", sampler, "MinFilter");
                updated |= ImGuiHelper.ComboFromEnum<GX2TexZFilterType>("Z Filter", sampler, "ZFilter");
                updated |= ImGuiHelper.ComboFromEnum<GX2TexMipFilterType>("Mip Filter", sampler, "MipFilter");
                updated |= ImGuiHelper.ComboFromEnum<GX2TexAnisoRatio>("Anisotropic Ratio", sampler, "MaxAnisotropicRatio");
            }
            if (ImGui.CollapsingHeader("Mip LOD", flags))
            {
                updated |= ImGuiHelper.InputFromFloat("Lod Min", sampler, "MinLod", false, 1);
                updated |= ImGuiHelper.InputFromFloat("Lod Max", sampler, "MaxLod", false, 1);
                updated |= ImGuiHelper.InputFromFloat("Lod Bias", sampler, "LodBias", false, 1);
            }
            if (ImGui.CollapsingHeader("Depth", flags))
            {
                updated |= ImGuiHelper.InputFromBoolean("Depth Enabled", sampler, "DepthCompareEnabled");
                updated |= ImGuiHelper.ComboFromEnum<GX2CompareFunction>("Depth Compare", sampler, "DepthCompareFunc");
            }
            if (ImGui.CollapsingHeader("Border", flags))
            {
                updated |= ImGuiHelper.ComboFromEnum<GX2TexBorderType>("Border Type", sampler, "BorderType");
            }
            return updated;
        }

        static STGenericTexture SearchTextureInstance(string name)
        {
            foreach (var cache in GLFrameworkEngine.DataCache.ModelCache.Values) {
                if (cache is BfresRender) {
                    if (((BfresRender)cache).Textures.ContainsKey(name))
                        return ((BfresRender)cache).Textures[name].OriginalSource;
                }
            }
            return null;
        }
    }
}
