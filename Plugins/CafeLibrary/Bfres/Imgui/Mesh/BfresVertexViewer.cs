using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary;
using MapStudio.UI;
using UIFramework;
using BfresLibrary.Helpers;
using ImGuiNET;

namespace CafeLibrary
{
    public class BfresVertexViewer : Window
    {
        public override string Name => "Vertices";

        public VertexBufferHelperAttrib ActiveAttribute = null;

        private VertexBufferHelper vertexBuffer;
        private List<int> selectedIndices = new List<int>();

        public BfresVertexViewer()
        {
            Size = new System.Numerics.Vector2(400, 700);
        }

        public void SelectShape(FSHP fshp)
        {
            vertexBuffer = new VertexBufferHelper(fshp.VertexBuffer, fshp.ModelWrapper.ResFile.ByteOrder);
        }

        public void SelectAttribute(string attribute)
        {
            ActiveAttribute = vertexBuffer.Attributes.FirstOrDefault(x => x.Name == attribute);
        }

        public override void Render()
        {
            base.Render();

            ImGui.Text($"Total Memory Size: {CalculateTotalMemorySize()}");
            ImGui.Text($"Buffer Memory Size: {CalculateMemorySize()}");

            string attribute = ActiveAttribute == null ? "" : $"{ActiveAttribute.Name} ({ModelImportSettings.GetAttributeName(ActiveAttribute.Name)})";
            if (ImGui.BeginCombo("##attributeSelector", attribute))
            {
                foreach (var att in vertexBuffer.Attributes)
                {
                    bool select = att == ActiveAttribute;
                    if (ImGui.Selectable($"{att.Name} ({ModelImportSettings.GetAttributeName(att.Name)})", select))
                        ActiveAttribute = att;
                    if (select)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (ActiveAttribute == null)
                return;

            ImGui.Text($"Format: {ModelImportSettings.GetFormatName(ActiveAttribute.Format)}");

            string[] elements = GetElements();
            ImGui.BeginColumns("vertexClmn", elements.Length + 1);
            ImGui.Text("ID");
            ImGui.NextColumn();

            for (int i = 0; i < elements.Length; i++)
            {
                ImGui.Text(elements[i]);
                ImGui.NextColumn();
            }
            ImGui.EndColumns();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0));

            float totalItemHeight = ImGui.GetFrameHeightWithSpacing() - 5;
            ImGuiNative.igSetNextWindowContentSize(new System.Numerics.Vector2(0.0f, ActiveAttribute.Data.Length * (totalItemHeight)));

            var childHeight = ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 3;

            if (ImGui.BeginChild("attributeList", new System.Numerics.Vector2(ImGui.GetWindowWidth(), childHeight)))
            {
                ImGui.BeginColumns("vertexClmn", elements.Length + 1);

                ImGuiListClipper2 clipper = new ImGuiListClipper2(ActiveAttribute.Data.Length, (totalItemHeight));
                for (int line_i = clipper.DisplayStart; line_i < clipper.DisplayEnd; line_i++) // display only visible items
                {
                    ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.15f);

                    bool selected = ImGui.Selectable($"{line_i.ToString()}", selectedIndices.Contains(line_i), ImGuiSelectableFlags.SpanAllColumns);
                    bool doubleClicked = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0);

                    if (selected)
                    {
                        selectedIndices.Clear();
                        selectedIndices.Add(line_i);
                    }

                    ImGui.NextColumn();
                    if (ActiveAttribute.Name.StartsWith("_c"))
                    {
                        for (int j = 0; j < elements.Length - 1; j++)
                        {
                            ImGui.Text($"{ActiveAttribute.Data[line_i][j]}");
                            ImGui.NextColumn();
                        }

                        var color = new System.Numerics.Vector4(
                            ActiveAttribute.Data[line_i][0],
                            ActiveAttribute.Data[line_i][1],
                            ActiveAttribute.Data[line_i][2],
                            ActiveAttribute.Data[line_i][3]);

                        var size = new System.Numerics.Vector2(ImGui.GetColumnWidth() - 23, ImGui.GetFrameHeight()); ;
                        ImGui.ColorButton($"##vertexclr{line_i}", color, ImGuiColorEditFlags.AlphaPreviewHalf, size);

                        ImGui.NextColumn();
                    }
                    else
                    {
                        for (int j = 0; j < elements.Length; j++)
                        {
                            ImGui.Text($"{ActiveAttribute.Data[line_i][j]}");
                            ImGui.NextColumn();
                        }
                    }
                }
                ImGui.EndColumns();
            }
            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        private string CalculateTotalMemorySize()
        {
            uint size = 0;
            foreach (var att in vertexBuffer.Attributes)
                size += ModelImportSettings.GetFormatStride(att.Format) * (uint)att.Data.Length;
            return Toolbox.Core.STMath.GetFileSize(size);
        }

        private string CalculateMemorySize()
        {
            uint size = ModelImportSettings.GetFormatStride(ActiveAttribute.Format) * (uint)ActiveAttribute.Data.Length;
            return Toolbox.Core.STMath.GetFileSize(size);
        }

        private string[] GetElements()
        {
            if (ActiveAttribute.Name.StartsWith("_u"))
                return new string[] { "X", "Y" };
            if (ActiveAttribute.Name.StartsWith("_c"))
                return new string[] { "R", "G", "B", "A", "Color" };
            if (ActiveAttribute.Name.StartsWith("_p") || ActiveAttribute.Name.StartsWith("_n"))
                return new string[] { "X", "Y", "Z" };

            return new string[] { "X", "Y", "Z", "W" };
        }
    }
}
