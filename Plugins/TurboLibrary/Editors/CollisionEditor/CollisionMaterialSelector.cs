using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using ImGuiNET;
using UIFramework;
using MapStudio.UI;
using Toolbox.Core;

namespace TurboLibrary
{
    public class CollisionMaterialSelector 
    {
        public ushort Attribute = 0;

        public EventHandler AttributeCalculated;

        private bool _openPopup = false;

        int IconSize = 17;

        Vector2 WindowPosition = new Vector2(0);
        public CollisionMaterialSelector()
        {
            foreach (var mat in Directory.GetFiles($"{Runtime.ExecutableDir}\\Lib\\Images\\Collision"))
                if (!IconManager.HasIcon(mat))
                    IconManager.TryAddIcon(System.IO.Path.GetFileNameWithoutExtension(mat), File.ReadAllBytes(mat));
        }

        public void OpenPopup()
        {
            _openPopup = true;
        }

        public string GetAttributeName(ushort attribute)
        {
            int attributeMaterial = (attribute & 0xFF);
            int materialIndex = attributeMaterial / 0x20;
            int attributeID = attributeMaterial - (materialIndex * 0x20);

            return CollisionCalculator.AttributeList[attributeID];
        }

        public string GetAttributeMaterialName(ushort attribute)
        {
            int attributeMaterial = (attribute & 0xFF);
            int materialIndex = attributeMaterial / 0x20;
            int attributeID = attributeMaterial - (materialIndex * 0x20);

            return CollisionCalculator.AttributeMaterials[attributeID][materialIndex];
        }

        public string GetSpecialTypeName(ushort attribute)
        {
            int specialFlag = (attribute >> 8);
            if (specialFlag == 0x10) return CollisionCalculator.SpecialType[1];
            if (specialFlag == 0x20) return CollisionCalculator.SpecialType[2];
            if (specialFlag == 0x40) return CollisionCalculator.SpecialType[3];
            if (specialFlag == 0x80) return CollisionCalculator.SpecialType[4];

            return "None";
        }

        public void Update(ushort attribute)
        {
            int specialFlag = (attribute >> 8);
            int attributeMaterial = (attribute & 0xFF);
            int materialIndex = attributeMaterial / 0x20;
            int attributeID = attributeMaterial - (materialIndex * 0x20);

            AttributeIdx = attributeID;
            MaterialAttributeIdx = materialIndex;
            SpecialTypeIdx = 0;
            if (specialFlag == 0x10) SpecialTypeIdx = 1;
            if (specialFlag == 0x20) SpecialTypeIdx = 2;
            if (specialFlag == 0x40) SpecialTypeIdx = 3;
            if (specialFlag == 0x80) SpecialTypeIdx = 4;

            Attribute = attribute;
        }

        public int AttributeIdx = 0;
        public int MaterialAttributeIdx = 0;
        public int SpecialTypeIdx = 0;
        public int HoveredAttributeIdx;

        public void Render()
        {
            var attributes = CollisionCalculator.AttributeList;
            var materials = CollisionCalculator.AttributeMaterials;
            var specialTypes = CollisionCalculator.SpecialType;

            void SelectAttribute(int index)
            {
                string icon = !IconManager.HasIcon(attributes[index]) ? "Attribute" : attributes[index];
                if (IconManager.HasIcon(icon))
                {
                    ImGui.Image((IntPtr)IconManager.GetTextureIcon(icon), new Vector2(IconSize, IconSize));
                    ImGui.SameLine();
                }
                else
                    return;

                if (ImGui.Selectable($"{attributes[index]}##att{index}", AttributeIdx == index))
                {
                    AttributeIdx = index;
                    MaterialAttributeIdx = 0;
                    Calculate();
                }
                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0) && AttributeIdx == index)
                {
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.IsItemFocused() && AttributeIdx != index)
                {
                    AttributeIdx = index;
                    MaterialAttributeIdx = 0;
                    Calculate();
                }
            }


            var color = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(color.X, color.Y, color.Z, 0.95f));

            ImGui.SetNextWindowPos(WindowPosition, ImGuiCond.Appearing);

            if (ImGui.BeginPopup("AttributePopup"))
            {
                ImGui.BeginChild("attributeChild", new Vector2(400, 550));

                //Update the window with the current position
                if (WindowPosition != Vector2.Zero)
                    WindowPosition = ImGui.GetWindowPos();

                ImGui.BeginColumns("attList", 2);

                MapStudio.UI.ImGuiHelper.BoldText("Attribute List");

                ImGui.BeginChild("attributeList");

                MapStudio.UI.ImGuiHelper.BoldText("Common-Roads");

                SelectAttribute(10);//Dash
                SelectAttribute(11);//Gravity Pad
                SelectAttribute(12);//Glider Pad

                SelectAttribute(28); //Fall Out

                string icon1 = !IconManager.HasIcon("Glider Activator") ? "Attribute" : "Glider Activator";
                if (IconManager.HasIcon(icon1))
                {
                    ImGui.Image((IntPtr)IconManager.GetTextureIcon(icon1), new Vector2(IconSize, IconSize));
                    ImGui.SameLine();
                }

                if (ImGui.Selectable("Glider Activator", (AttributeIdx == 31 && MaterialAttributeIdx == 0)))
                {
                    //Select zone and glider combo
                    AttributeIdx = 31;
                    MaterialAttributeIdx = 0;
                    Calculate();
                }
                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0) && AttributeIdx == 31)
                {
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.IsItemFocused() && AttributeIdx != 31)
                {
                    //Select zone and glider combo
                    AttributeIdx = 31;
                    MaterialAttributeIdx = 0;
                    Calculate();
                }

                SelectAttribute(9);//Slippery

                SelectAttribute(15); //Item Road
                SelectAttribute(16); //Lakitu Rescue
                SelectAttribute(31); //Zone

                MapStudio.UI.ImGuiHelper.BoldText("Common-Walls");

                SelectAttribute(21); //Item Wall
                SelectAttribute(23); //Invisible Wall

                MapStudio.UI.ImGuiHelper.BoldText("Primary Roads");
                for (int i = 0; i < 4; i++)
                    SelectAttribute(i);

                MapStudio.UI.ImGuiHelper.BoldText("Primary Walls");

                for (int i = 17; i < 20; i++)
                    SelectAttribute(i);

                MapStudio.UI.ImGuiHelper.BoldText("Off-Roads");
                for (int i = 4; i < 9; i++)
                    SelectAttribute(i);



                MapStudio.UI.ImGuiHelper.BoldText("Special-Walls");

                SelectAttribute(20); //LWALL
                SelectAttribute(22); //BWALL

                MapStudio.UI.ImGuiHelper.BoldText("Special-Roads");

                for (int i = 13; i < 15; i++)
                    SelectAttribute(i);

                MapStudio.UI.ImGuiHelper.BoldText("Special-Attributes");

                SelectAttribute(26);
                SelectAttribute(27);
                SelectAttribute(29);
                SelectAttribute(30);

                ImGui.EndChild();

                ImGui.NextColumn();

                MapStudio.UI.ImGuiHelper.BoldText("Material List");

                for (int i = 0; i < materials[AttributeIdx].Length; i++)
                {
                    string mat = materials[AttributeIdx][i];
                    if (mat == "None")
                        continue;

                    string icon = !IconManager.HasIcon(mat) ?  "Material" : mat;
                    if (IconManager.HasIcon(icon))
                    {
                        ImGui.Image((IntPtr)IconManager.GetTextureIcon(icon), new Vector2(IconSize, IconSize));
                        ImGui.SameLine();
                    }
                    else
                        continue;

                    if (ImGui.Selectable($"{mat}##attmat{i}", i == MaterialAttributeIdx))
                    {
                        MaterialAttributeIdx = i;
                        Calculate();
                    }
                    if (ImGui.IsItemFocused() && MaterialAttributeIdx != i)
                    {
                        MaterialAttributeIdx = i;
                        Calculate();
                    }
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0) && MaterialAttributeIdx == i)
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.NextColumn();

                ImGui.EndColumns();

                ImGui.EndChild();
                ImGui.EndPopup();
            }
            ImGui.PopStyleColor();

            ImGui.BeginColumns("attributeSelector", 3);

            ImGui.AlignTextToFramePadding();
            MapStudio.UI.ImGuiHelper.BoldText($"Attribute:");

            ImGui.SameLine();

            string att = attributes[AttributeIdx];

            ImGui.PushItemWidth(150);
            if (ImGui.BeginCombo("##attributeCB", att))
            {
                ImGui.EndCombo();
            }

            if (WindowPosition == Vector2.Zero)
                WindowPosition = ImGui.GetCursorScreenPos();

            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(0) || _openPopup)
            {
                ImGui.OpenPopup("AttributePopup");
                _openPopup = false;
            }
            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            MapStudio.UI.ImGuiHelper.BoldText($"Material:");
            ImGui.SameLine();
            if (ImGui.BeginCombo("##Material", materials[AttributeIdx][MaterialAttributeIdx], ImGuiComboFlags.HeightLargest))
            {
                for (int i = 0; i < materials[AttributeIdx].Length; i++)
                {
                    string mat = materials[AttributeIdx][i];
                    if (IconManager.HasIcon(mat))
                    {
                        ImGui.Image((IntPtr)IconManager.GetTextureIcon(mat), new Vector2(20, 20));
                        ImGui.SameLine();
                    }

                    if (ImGui.Selectable($"{materials[AttributeIdx][i]}##attmat{i}", i == MaterialAttributeIdx))
                    {
                        MaterialAttributeIdx = i;
                        Calculate();
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            MapStudio.UI.ImGuiHelper.BoldText($"Special: ");
            ImGui.SameLine();

            if (ImGui.BeginCombo("##Special", specialTypes[SpecialTypeIdx], ImGuiComboFlags.HeightLargest))
            {
                for (int i = 0; i < specialTypes.Length; i++)
                {
                    if (ImGui.Selectable(specialTypes[i], i == SpecialTypeIdx))
                    {
                        SpecialTypeIdx = i;
                        Calculate();
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.PopItemWidth();

            ImGui.NextColumn();

            ImGui.EndColumns();
        }

        public void Calculate()
        {
            Attribute = (ushort)AttributeIdx; //general type
            Attribute += (ushort)(MaterialAttributeIdx * 0x20); //material (effect + sound)

            //trick
            if (SpecialTypeIdx == 1) Attribute += 0x1000;
            if (SpecialTypeIdx == 2) Attribute += 0x2000;
            if (SpecialTypeIdx == 3) Attribute += 0x4000;
            if (SpecialTypeIdx == 4) Attribute += 0x8000;

            AttributeCalculated?.Invoke(this, EventArgs.Empty);
        }

        private void RenderMaterials(int attributeIdx)
        {
            var materials = CollisionCalculator.AttributeMaterials;
            string attribute = CollisionCalculator.AttributeList[attributeIdx];

            foreach (var mat in materials[attributeIdx].OrderBy(x => x))
            {
                if (mat == "None")
                    continue;

                if (CollisionCalculator.AttributeMatColors.ContainsKey(mat))
                {
                    var color = CollisionCalculator.AttributeMatColors[mat];
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.X, color.Y, color.Z, color.W));
                    ImGui.Selectable(mat);
                    ImGui.PopStyleColor();
                }
                else if (CollisionCalculator.AttributeColors.ContainsKey(attribute))
                {
                    var color = CollisionCalculator.AttributeColors[attribute];
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.X, color.Y, color.Z, color.W));
                    ImGui.Selectable(mat);
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.Selectable(mat);
                }
            }
        }
    }
}
