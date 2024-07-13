using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;

namespace CafeLibrary
{
    public class BfresMaterialEditor
    {
        public static bool exportTextures = false;

        FMAT activeMaterial;

        static bool onLoad = false;

        static UVViewport UVViewport = null;

        static MaterialPresetWindow PresetWindow = new MaterialPresetWindow();

        TabControl TabControl = new TabControl("material_menu1");
        ShaderAttributeMapper attributeMapper = new ShaderAttributeMapper();

        public void Init()
        {
            UVViewport = new UVViewport();
            UVViewport.Camera.Zoom = 30;
            UVViewport.OnLoad();
        }

        public void LoadEditor(FMAT material) {
            LoadEditorMenus(material);
        }

        private string presetName = "";

        public void LoadEditorMenus(FMAT material)
        {
            //If the user adds a model with mapped materials that aren't in the file
            //Load just the preset UI for configuring the material to use.
            if (material.IsMaterialInvalid) {
                var width = ImGui.GetWindowWidth();
                var size = new System.Numerics.Vector2(width, 23);
                if (ImGui.Button("Select Preset Preset", size))
                {
                    //reload preset folder
                    PresetWindow.LoadPresets(material.ResFile.IsPlatformSwitch);

                    DialogHandler.Show("Import Preset", 400, 600, () =>
                    {
                        PresetWindow.Render();
                    }, (ok) =>
                    {
                        if (ok)
                        {
                            var fmdl = material.UINode.Parent.Parent.Tag as FMDL;
                            foreach (FMAT mat in fmdl.GetSelectedMaterials())
                                mat.LoadPreset(PresetWindow.GetSelectedPreset(), PresetWindow.KeepTextures());
                        }
                    });
                }
                return;
            }

            if (ImGui.CollapsingHeader("Preset", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var width = ImGui.GetWindowWidth() / 2;
                var size = new System.Numerics.Vector2(23, 23);
                if (ImGui.Button($"  {IconManager.SAVE_ICON}     ", size))
                {
                    if (string.IsNullOrEmpty(presetName))
                        TinyFileDialog.MessageBoxInfoOk($"Must set a name for the material preset!");
                    else
                    {
                        material.SaveAsPreset(Path.Combine(Toolbox.Core.Runtime.ExecutableDir,"Presets","Materials",$"{presetName}.json"), exportTextures);
                        TinyFileDialog.MessageBoxInfoOk($"Saved material preset {presetName}!");
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button($"   {IconManager.OPEN_ICON}   ##materialPresetEdit", size))
                {
                    //reload preset folder
                    PresetWindow.LoadPresets(material.ResFile.IsPlatformSwitch);

                    DialogHandler.Show("Import Preset", 400, 600, () =>
                    {
                        PresetWindow.Render();
                    }, (ok) => 
                    {
                        if (ok)
                        {
                            var fmdl = material.UINode.Parent.Parent.Tag as FMDL;
                            foreach (FMAT mat in fmdl.GetSelectedMaterials())
                                mat.LoadPreset(PresetWindow.GetSelectedPreset(), PresetWindow.KeepTextures());
                        }
                    });
                }
                ImGui.SameLine();
                ImGui.InputText("Preset Name", ref presetName, 0x100);
                ImGui.Checkbox($"  {'\uf03e'}     Export Textures", ref exportTextures);
            }

            if (UVViewport == null)
                Init();

            if (activeMaterial != material)
            {
                onLoad = true;
                MaterialParameter.Reset();
                MaterialOptions.Reset();
                BfresTextureMapEditor.Reset();
                UVViewport.Reset();
                presetName = "";
                activeMaterial = material;

                if (TabControl.Pages.Count == 0)
                    PrepareTabs();
            }

            if (ImGui.CollapsingHeader("Material Info", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.InputFromText("Name", material, "Name", 200);
                ImGuiHelper.InputFromText("ShaderArchive", material, "ShaderArchive", 200);
                ImGuiHelper.InputFromText("ShaderModel", material, "ShaderModel", 200);
                if (ImGui.Button("Sampler Shader Mapper"))
                {
                    attributeMapper.Load(material.Material.ShaderAssign.SamplerAssigns);
                    DialogHandler.Show(attributeMapper.Name, attributeMapper.Size.X, attributeMapper.Size.Y, () =>
                    {
                        attributeMapper.Render();
                    }, (ok) =>
                    {
                        material.Material.ShaderAssign.SamplerAssigns = attributeMapper.ToResDict();
                        material.ReloadSamplers();
                    });
                }
                ImGui.SameLine();
                if (ImGui.Button("Attributes Shader Mapper"))
                {
                    attributeMapper.Load(material.Material.ShaderAssign.AttribAssigns);
                    DialogHandler.Show(attributeMapper.Name, attributeMapper.Size.X, attributeMapper.Size.Y, () =>
                    {
                        attributeMapper.Render();
                    }, (ok) =>
                    {
                        material.Material.ShaderAssign.AttribAssigns = attributeMapper.ToResDict();
                        material.ReloadAttributes();
                    });
                }
            }
            ImGuiHelper.InputFromBoolean("Visible", material.Material, "Visible");

            if (ImGui.BeginChild("##MATERIAL_EDITOR"))
            {
                TabControl.Render();
            }
            ImGui.EndChild();

            onLoad = false;
        }


        private void PrepareTabs()
        {
            TabControl.Pages.Clear();
            TabControl.Pages.Add(new TabPage($"   {'\uf302'}    Textures", () =>
            {
                BfresTextureMapEditor.Render(activeMaterial, UVViewport, onLoad);
            }));
            TabControl.Pages.Add(new TabPage($"   {'\uf0ad'}    Params", () =>
            {
                MaterialParameter.Render(activeMaterial);
            }));
            TabControl.Pages.Add(new TabPage($"   {'\uf5fd'}    Render Info", () =>
            {
                RenderInfoEditor.Render(activeMaterial, activeMaterial.Material.RenderInfos);
            }));
            TabControl.Pages.Add(new TabPage($"   {'\uf126'}    Options", () =>
            {
                MaterialOptions.Render(activeMaterial);
            }));
            if (!activeMaterial.ResFile.IsPlatformSwitch)
            {
                TabControl.Pages.Add(new TabPage($"   {'\uf06e'}    Render State", () =>
                {
                    RenderStateEditor.Render(activeMaterial);
                }));
            }
            TabControl.Pages.Add(new TabPage($"   {'\uf03a'}    User Data", () =>
            {
                UserDataInfoEditor.Render(activeMaterial.Material.UserData);
            }));
            if (activeMaterial.MaterialAsset is Rendering.BfshaRenderer)
            {
                TabControl.Pages.Add(new TabPage($"   {'\uf61f'}    Shaders", () =>
                {
                    BfshaShaderProgramViewer.Render(activeMaterial);
                }));
            }
        }
    }
}
