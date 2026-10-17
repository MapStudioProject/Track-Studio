using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using Toolbox.Core;
using MapStudio.UI;
using CafeLibrary.Rendering;
using TurboLibrary;
using PartsEditor;
using TurboLibrary.Parts;

namespace CafeLibrary
{
    public class PartSelectionDialog
    {
        public static string OutputName = "";
        public static string Previous = "";

        static string _searchText = "";

        static bool popupOpened = false;
        static bool scrolled = false;

        public static void Init() { OutputName = ""; }

        static bool IDNames = false;
        static bool FilterBikes = false;
        static bool FilterAtv = false;
        static bool FilterKarts = false;

        public static bool Render(string input, ref bool dialogOpened)
        {
            if (string.IsNullOrEmpty(OutputName))
            {
                OutputName = input;
                Previous = input;
            }

            var pos = ImGui.GetCursorScreenPos();

            if (!popupOpened)
            {
                ImGui.OpenPopup("partSelector1");
                popupOpened = true;
                scrolled = false;
            }

            ImGui.SetNextWindowPos(pos, ImGuiCond.Appearing);

            var color = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(color.X, color.Y, color.Z, 1.0f));

            bool hasInput = false;
            if (ImGui.BeginPopup("partSelector1", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.IsKeyDown((int)ImGuiKey.Enter))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"   {IconManager.SEARCH_ICON}  ");

                ImGui.SameLine();

                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                if (ImGui.InputText("Search", ref _searchText, 200))
                {
                }
                ImGui.PopStyleVar();

                ImGui.Checkbox("Use ID Names", ref IDNames);

                if (ImGui.Checkbox("Filter Karts", ref FilterKarts))
                {
                    FilterBikes = false;
                    FilterAtv = false;
                }
                ImGui.SameLine();

                if (ImGui.Checkbox("Filter Atv", ref FilterAtv))
                {
                    FilterKarts = false;
                    FilterBikes = false;
                }
                ImGui.SameLine();

                if (ImGui.Checkbox("Filter Bikes", ref FilterBikes))
                {
                    FilterKarts = false;
                    FilterAtv = false;
                }

                var width = ImGui.GetWindowWidth();

                float size = ImGui.GetFrameHeight();
                ImGui.BeginChild("partList", new System.Numerics.Vector2(320, 300));
                bool isSearch = !string.IsNullOrEmpty(_searchText);

                for (int i = 0; i < MushResParts.KartNameList.Count; i++)
                {
                    string key = ((EBodyID)i).ToString();
                    string text = IDNames ? key : MushResParts.KartNameList[i];

                    bool HasText = text.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isSearch && !HasText)
                        continue;

                    if (!key.Contains("BodyK") && FilterKarts)
                        continue;

                    if (!key.Contains("BodyB") && FilterBikes)
                        continue;

                    if (!key.Contains("BodyV") && FilterAtv)
                        continue;

                    bool isSelected = OutputName == text;

                    if (!scrolled && isSelected)
                    {
                        ImGui.SetScrollHereY();
                        scrolled = true;
                    }

                    if (ImGui.Selectable(text, isSelected))
                    {
                        OutputName = key;
                        hasInput = true;
                    }
                    if (ImGui.IsMouseDoubleClicked(0) && ImGui.IsItemHovered())
                    {
                        ImGui.CloseCurrentPopup();
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndChild();

                ImGui.EndPopup();
            }
            else if (popupOpened)
            {
                dialogOpened = false;
                popupOpened = false;
            }
            ImGui.PopStyleColor();

            return hasInput;
        }
    }
}
