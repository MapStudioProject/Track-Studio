using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using BfresLibrary;
using MapStudio.UI;

namespace CafeLibrary
{
    public class RenderInfoDialog
    {
        public List<string> ValuePresets = new List<string>();

        public bool canParse = true;

        private string ValuesEdit = "";

        public void Load(FMAT material, RenderInfo renderInfo)
        {
            ValuesEdit = GetDataString(renderInfo);
            if (string.IsNullOrEmpty(ValuesEdit))
                ValuesEdit = "";
            ValuePresets = RenderInfoEnums.FindRenderInfoPresets(material, renderInfo.Name).ToList();
        }

        public void Render(RenderInfo renderInfo)
        {
            if (!canParse)
                ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), $"Failed to parse type {renderInfo.Type}!");

            ImGui.PushItemWidth(ImGui.GetWindowWidth());
            ImGuiHelper.InputFromText(TranslationSource.GetText("NAME"), renderInfo, "Name", 200);

            if (ValuePresets.Count > 0 && renderInfo.Type == RenderInfoType.String)
            {
                string value = renderInfo.GetValueStrings()[0];
                if (ImGui.BeginCombo("Presets", ""))
                {
                    foreach (var val in ValuePresets)
                    {
                        bool isSelected = val == value;
                        if (ImGui.Selectable(val, isSelected)) {
                            renderInfo.SetValue(new string[1] { val });
                            ValuesEdit = val;
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
            }

            bool formatChanged = ImGuiHelper.ComboFromEnum<RenderInfoType>(TranslationSource.GetText("TYPE"), renderInfo, "Type");
            if (formatChanged)
                UpdateValues(renderInfo);

            var windowSize = ImGui.GetWindowSize();
            var textSize = new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 32);

            if (ImGui.InputTextMultiline(TranslationSource.GetText("VALUE"), ref ValuesEdit, 0x1000, textSize))
            {
                UpdateValues(renderInfo);
            }
            ImGui.PopItemWidth();

            ImGui.SetCursorPosY(windowSize.Y - 28);

            var buttonSize = new Vector2(ImGui.GetWindowWidth() / 2 - 2, ImGui.GetFrameHeight());
            if (ImGui.Button("Cancel", buttonSize))
            {
                DialogHandler.ClosePopup(false);
            }
            ImGui.SameLine();
            if (ImGui.Button("Ok", buttonSize))
            {
                if (canParse && !string.IsNullOrEmpty(renderInfo.Name))
                    DialogHandler.ClosePopup(true);
            }
        }

        void UpdateValues(RenderInfo renderInfo)
        {
            canParse = true;
            string[] values = ValuesEdit.Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (renderInfo.Type == RenderInfoType.Int32)
            {
                int[] data = new int[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    canParse = int.TryParse(values[i], out int value);
                    if (!canParse)
                        return;

                    data[i] = value;
                }
                renderInfo.SetValue(data);
            }
            if (renderInfo.Type == RenderInfoType.Single)
            {
                float[] data = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    canParse = float.TryParse(values[i], out float value);
                    if (!canParse)
                        return;

                    data[i] = value;
                }
                renderInfo.SetValue(data);
            }
            else
            {
                string[] data = new string[values.Length];
                for (int i = 0; i < values.Length; i++)
                    data[i] = values[i];
                renderInfo.SetValue(data);
            }
        }

        static string GetDataString(RenderInfo renderInfo, string seperator = "\n")
        {
            if (renderInfo.Type == RenderInfoType.Int32)
                return string.Join(seperator, renderInfo.GetValueInt32s());
            if (renderInfo.Type == RenderInfoType.Single)
                return string.Join(seperator, renderInfo.GetValueSingles());
            else
                return string.Join(seperator, renderInfo.GetValueStrings());

        }
    }
}
