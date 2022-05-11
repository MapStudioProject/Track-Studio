using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ImGuiNET;
using UIFramework;
using MapStudio.UI;
using System.Numerics;
using Toolbox.Core;

namespace TurboLibrary
{
    public class CollisionPainterUI : DockWindow
    {
        public override string Name => "Collision Painter";

        public CollisionPainterUI(DockSpaceWindow window) : base(window)
        {
            this.DockDirection = ImGuiDir.Right;
            this.SplitRatio = 0.5f;

            foreach (var mat in Directory.GetFiles(System.IO.Path.Combine(Runtime.ExecutableDir,"Images","Collision")))
                if (!IconManager.HasIcon(mat))
                    IconManager.TryAddIcon(System.IO.Path.GetFileNameWithoutExtension(mat), File.ReadAllBytes(mat));
        }

        private int MaterialAttributeIdx;
        private int AttributeIdx;

        public override void Render()
        {
            if (ImGui.BeginChild("collisonPainter"))
            {
                for (int i = 0; i < CollisionCalculator.AttributeList.Length; i++)
                    DrawAttribute(i);
            }
            ImGui.EndChild();
        }

        private void DrawAttribute(int attributeIdx)
        {
            var attributeName = CollisionCalculator.AttributeList[attributeIdx];

            string icon = !IconManager.HasIcon(attributeName) ? "Attribute" : attributeName;
            if (IconManager.HasIcon(icon))
            {
                ImGui.Image((IntPtr)IconManager.GetTextureIcon(icon), new Vector2(20, 20));
                ImGui.SameLine();
            }

            if (ImGui.CollapsingHeader(attributeName))
            {
                DrawMaterials(attributeIdx);
            }
        }

        private void DrawMaterials(int attributeIdx)
        {
            var materials = CollisionCalculator.AttributeMaterials;
            var attribute = CollisionCalculator.AttributeList[attributeIdx];

            for (int i = 0; i < materials[attributeIdx].Length; i++)
            {
                string mat = materials[attributeIdx][i];
                if (mat == "None")
                    continue;

                string icon = !IconManager.HasIcon(mat) ? "Material" : mat;
                if (IconManager.HasIcon(icon))
                {
                    ImGui.Image((IntPtr)IconManager.GetTextureIcon(icon), new Vector2(20, 20));
                    ImGui.SameLine();
                }

                OpenTK.Vector4 color = new OpenTK.Vector4(0.5f, 0.5f, 0.5f, 1.0f);

                if (CollisionCalculator.AttributeColors.ContainsKey(attribute))
                    color = CollisionCalculator.AttributeColors[attribute];

                if (CollisionCalculator.AttributeMatColors.ContainsKey(mat))
                    color = CollisionCalculator.AttributeMatColors[mat];

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1.0f));

                var pos = ImGui.GetCursorScreenPos();
                ImGui.GetWindowDrawList().AddRectFilled(
                    new Vector2(pos.X, pos.Y),
                    new Vector2(pos.X + ImGui.GetWindowWidth(), pos.Y + ImGui.GetFrameHeight()),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, color.W)));

                bool select = AttributeIdx == attributeIdx && i == MaterialAttributeIdx;
                if (ImGui.Selectable($"{materials[attributeIdx][i]}##attmat{i}", select))
                {
                    MaterialAttributeIdx = i;
                    AttributeIdx = attributeIdx;
                }
                ImGui.PopStyleColor();
            }
        }
    }
}
