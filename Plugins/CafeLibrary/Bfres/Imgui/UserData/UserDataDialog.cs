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
    public class UserDataDialog
    {
        public List<string> ValuePresets = new List<string>();

        public bool canParse = true;

        private string ValuesEdit = "";

        public void Load(UserData userData)
        {
            ValuesEdit = GetDataString(userData);
            if (string.IsNullOrEmpty(ValuesEdit))
                ValuesEdit = "";
        }

        public void Render(UserData userData)
        {
            if (!canParse)
                ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), $"Failed to parse type {userData.Type}!");

            ImGui.PushItemWidth(ImGui.GetWindowWidth());
            ImGuiHelper.InputFromText(TranslationSource.GetText("NAME"), userData, "Name", 200);
            bool formatChanged = ImGuiHelper.ComboFromEnum<UserDataType>(TranslationSource.GetText("TYPE"), userData, "Type");
            if (formatChanged)
                UpdateValues(userData);

            var windowSize = ImGui.GetWindowSize();
            var textSize = new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 32);

            if (ImGui.InputTextMultiline(TranslationSource.GetText("VALUE"), ref ValuesEdit, 0x1000, textSize))
            {
                UpdateValues(userData);
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
                if (canParse && !string.IsNullOrEmpty(userData.Name))
                    DialogHandler.ClosePopup(true);
            }
        }

        void UpdateValues(UserData userData)
        {
            canParse = true;
            string[] values = ValuesEdit.Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (userData.Type == UserDataType.Int32)
            {
                int[] data = new int[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    canParse = int.TryParse(values[i], out int value);
                    if (!canParse)
                        return;

                    data[i] = value;
                }
                userData.SetValue(data);
            }
            else if (userData.Type == UserDataType.Byte)
            {
                byte[] data = new byte[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    canParse = byte.TryParse(values[i], out byte value);
                    if (!canParse)
                        return;

                    data[i] = value;
                }
                userData.SetValue(data);
            }
            else if (userData.Type == UserDataType.Single)
            {
                float[] data = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    canParse = float.TryParse(values[i], out float value);
                    if (!canParse)
                        return;

                    data[i] = value;
                }
                userData.SetValue(data);
            }
            else
            {
                string[] data = new string[values.Length];
                for (int i = 0; i < values.Length; i++)
                    data[i] = values[i];
                userData.SetValue(data);
            }
        }

        static string GetDataString(UserData userData, string seperator = "\n")
        {
            if (userData.Type == UserDataType.Byte)
                return string.Join(seperator, userData.GetValueByteArray());
            else if (userData.Type == UserDataType.Int32)
                return string.Join(seperator, userData.GetValueInt32Array());
            else if (userData.Type == UserDataType.Single)
                return string.Join(seperator, userData.GetValueSingleArray());
            else
                return string.Join(seperator, userData.GetValueStringArray());

        }
    }
}
