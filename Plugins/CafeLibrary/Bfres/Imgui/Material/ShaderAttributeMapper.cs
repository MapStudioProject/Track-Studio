using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using BfresLibrary;
using ImGuiNET;
using UIFramework;
using MapStudio.UI;

namespace CafeLibrary
{
    public class ShaderAttributeMapper : Window
    {
        Dictionary<string, string> Value = new Dictionary<string, string>();

        int selectedIndex = 0;
        string selectedKey = "";

        public void Load(ResDict<ResString> mapper)
        {
            Value.Clear();
            foreach (var map in mapper)
                Value.Add(map.Key, map.Value);

            Size = new System.Numerics.Vector2(400, 350);
            selectedIndex = 0;
            selectedKey = mapper.Keys.FirstOrDefault();
        }

        public ResDict<ResString> ToResDict()
        {
            var dict = new ResDict<ResString>();
            foreach (var map in Value)
                dict.Add(map.Key, map.Value);
            return dict;
        }

        public override void Render()
        {
            if (ImGui.Button($"   {IconManager.ADD_ICON}   "))
            {
                if (!Value.ContainsKey(""))
                    Value.Add("", "");

                selectedIndex = Value.Count - 1;
                selectedKey = Value.Keys.LastOrDefault();
            }

            ImGui.SameLine();
            if (ImGui.Button($"   {IconManager.DELETE_ICON}   "))
            {
                if (selectedIndex != -1)
                {
                    var selentry = Value.ElementAt(selectedIndex);
                    Value.Remove(selentry.Key);
                }
                selectedIndex = Value.Count - 1;
                selectedKey = Value.Keys.LastOrDefault();
            }

            ImGui.BeginChild("shaderWindow1");

            var height = ImGui.GetWindowHeight();
            var width = ImGui.GetWindowWidth();

            ImGui.BeginColumns("shdrattcolumns", 2);

            RenderList();
            ImGui.NextColumn();

            ImGui.BeginChild("attributeData", new Vector2(ImGui.GetColumnWidth() - 3, height - 33));

            string resName = Value[selectedKey];
            string shaderName = new string(selectedKey);

            ImGui.PushItemWidth(100);
            if (ImGui.InputText("Bfres Input", ref resName, 0x100))
                Value[selectedKey] = resName;

            bool hasError = false;
            if (ImGui.InputText("Shader Input", ref shaderName, 0x100))
            {
                if (Value.ContainsKey(shaderName))
                    hasError = true;
                else
                {
                    Value.Remove(selectedKey);
                    Value.Add(shaderName, resName);
                    selectedKey = shaderName;
                }
            }

            if (hasError)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(ThemeHandler.Theme.Error));
                ImGui.TextWrapped($"A shader input with the same name ({shaderName}) already exists!");
                ImGui.PopStyleColor();

                shaderName = selectedKey;
            }

            ImGui.PopItemWidth();

            ImGui.EndChild();

            ImGui.NextColumn();
            ImGui.EndColumns();

            ImGui.SetCursorPosY(height - 34);

            if (ImGui.Button("Apply", new Vector2(width / 2, 30)))
            {
                DialogHandler.ClosePopup(true);
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(width / 2, 30)))
            {
                DialogHandler.ClosePopup(false);
            }

            ImGui.EndChild();
        }

        private void RenderList()
        {
            var height = ImGui.GetWindowHeight();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            if (ImGui.BeginChild("attributeList", new Vector2(ImGui.GetColumnWidth() - 3, height - 33)))
            {
                ImGui.BeginColumns("attList1", 2);

                MapStudio.UI.ImGuiHelper.BoldText("Bfres");
                ImGui.NextColumn();
                MapStudio.UI.ImGuiHelper.BoldText("Shader");
                ImGui.NextColumn();

                int i = 0;
                foreach (var map in Value)
                {
                    if (ImGui.Selectable($"{map.Value}##attMap{i}", selectedIndex == i, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        selectedIndex = i;
                        selectedKey = map.Key;
                    }
                    if (ImGui.IsItemFocused() && selectedIndex != i)
                    {
                        selectedIndex = i;
                        selectedKey = map.Key;
                    }

                    ImGui.NextColumn();
                    ImGui.Text(map.Key);
                    ImGui.NextColumn();
                    i++;
                }
                ImGui.EndColumns();
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();
        }
    }
}
