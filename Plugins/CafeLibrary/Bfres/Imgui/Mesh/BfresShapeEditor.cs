using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;

namespace CafeLibrary
{
    public class BfresShapeEditor
    {
        BfresMaterialEditor materialEditor = new BfresMaterialEditor();
        static BfresVertexViewer bfresVertexViewer;
        string selectAttribute = "";
        int selectedLOD = -1;

        public void Render(FSHP fshp)
        {
            ImGui.BeginTabBar("meshTab");
            if (ImguiCustomWidgets.BeginTab("meshTab", "Shape Data"))
            {
                RenderShapeUI(fshp);
                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("meshTab", "Material Data"))
            {
                materialEditor.LoadEditor(fshp.Material);
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }

        private void RenderShapeUI(FSHP fshp)
        {
            if (ImGui.CollapsingHeader("Shape Info", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.InputFromText("Name", fshp, "Name", 0x200);

                int skinCount = (int)fshp.VertexSkinCount;
                ImGui.InputInt("Skin Count", ref skinCount, 0);
            }
            if (ImGui.CollapsingHeader("Material Info", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginCombo("Material", fshp.Material.Name))
                {
                    foreach (FMAT mat in fshp.ModelWrapper.Materials)
                    {
                        bool select = mat == fshp.Material;
                        if (ImGui.Selectable(mat.Name, select)) {
                            fshp.AssignMaterial(mat);
                        }
                        if (select)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndCombo();
                }
                if (ImGui.IsItemHovered()) //Check for combo box hover
                {
                    var delta = ImGui.GetIO().MouseWheel;
                    if (delta < 0) //Check for mouse scroll change going up
                    {
                        int index = fshp.ModelWrapper.Materials.IndexOf(fshp.Material);
                        if (index < fshp.ModelWrapper.Materials.Count - 1)
                        { //Shift upwards if possible
                            fshp.AssignMaterial((FMAT)fshp.ModelWrapper.Materials[index + 1]);
                        }
                    }
                    if (delta > 0) //Check for mouse scroll change going down
                    {
                        int index = fshp.ModelWrapper.Materials.IndexOf(fshp.Material);
                        if (index > 0)
                        { //Shift downwards if possible
                            fshp.AssignMaterial((FMAT)fshp.ModelWrapper.Materials[index - 1]);
                        }
                    }
                }
            }
            if (ImGui.CollapsingHeader("Bone Info", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var bone = fshp.ModelWrapper.Skeleton.Bones[fshp.Shape.BoneIndex];
                if (ImGui.BeginCombo("Bone", bone.Name))
                {
                    foreach (var bn in fshp.ModelWrapper.Skeleton.Bones)
                    {
                        bool select = bn == bone;
                        if (ImGui.Selectable(bn.Name, select))
                        {
                            fshp.Shape.BoneIndex = (ushort)fshp.ModelWrapper.Skeleton.Bones.IndexOf(bn);
                        }
                        if (select)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
            }
            if (ImGui.CollapsingHeader("Vertex Info"))
            {
                int vertexCount = (int)fshp.Vertices.Count;
                ImGui.InputInt("Vertex Count", ref vertexCount, 0);

                ImGui.Columns(3);

                ImGuiHelper.BoldText("Name");
                ImGui.NextColumn();
                ImGuiHelper.BoldText("Hint");
                ImGui.NextColumn();
                ImGuiHelper.BoldText("Format");
                ImGui.NextColumn();

                foreach (var attribute in fshp.VertexBuffer.Attributes.Values)
                {
                    bool select = attribute.Name == selectAttribute;

                    bool selected = ImGui.Selectable(attribute.Name, select, ImGuiSelectableFlags.SpanAllColumns);
                    bool doubleClicked = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0);

                    ImGui.NextColumn();
                    ImGui.Text(ModelImportSettings.GetAttributeName(attribute.Name));
                    ImGui.NextColumn();
                    ImGui.Text(ModelImportSettings.GetFormatName(attribute.Format));
                    ImGui.NextColumn();

                    if (selected)
                        selectAttribute = attribute.Name;
                    if (doubleClicked) {
                        if(bfresVertexViewer == null)
                            bfresVertexViewer = new BfresVertexViewer();
                        bfresVertexViewer.SelectShape(fshp);
                        bfresVertexViewer.SelectAttribute(attribute.Name);
                        bfresVertexViewer.Opened = true;
                        if (!Workspace.ActiveWorkspace.Windows.Contains(bfresVertexViewer))
                            Workspace.ActiveWorkspace.Windows.Add(bfresVertexViewer);
                    }
                }
                ImGui.Columns(1);
            }
            if (ImGui.CollapsingHeader("Level of Detail"))
            {
                for (int i = 0; i < fshp.Shape.Meshes.Count; i++)
                {
                    if (ImGui.Selectable($"LOD_{i}", selectedLOD == i) || (ImGui.IsItemFocused() && selectedLOD != i))
                    {
                        selectedLOD = i;
                        if (i == 0)
                            fshp.MeshAsset.ForceLevelDetailIndex = -1;
                        else
                            fshp.MeshAsset.ForceLevelDetailIndex = i;
                        GLFrameworkEngine.GLContext.ActiveContext.UpdateViewport = true;
                    }
                }
            }
            if (ImGui.CollapsingHeader("Shape Keys"))
            {

            }
        }
    }
}
