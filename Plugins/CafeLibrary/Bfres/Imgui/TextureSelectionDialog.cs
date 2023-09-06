using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using Toolbox.Core;
using MapStudio.UI;
using CafeLibrary.Rendering;

namespace CafeLibrary
{
    public class TextureSelectionDialog
    {
        public static Dictionary<string, GLFrameworkEngine.GenericRenderer.TextureView> Textures = new Dictionary<string, GLFrameworkEngine.GenericRenderer.TextureView>();

        public static string OutputName = "";
        public static string Previous = "";

        static string _searchText = "";

        static bool popupOpened = false;
        static bool scrolled = false;

        public static void Init() { OutputName = ""; }

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
                ImGui.OpenPopup("textureSelector1");
                popupOpened = true;
                scrolled = false;
            }

            ImGui.SetNextWindowPos(pos, ImGuiCond.Appearing);

            var color = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(color.X, color.Y, color.Z, 1.0f));

            bool hasInput = false;
            if (ImGui.BeginPopup("textureSelector1", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"   {IconManager.EDIT_ICON}  ");

                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                ImGui.InputText("##OutputName", ref OutputName, 0x100);

                if (ImGui.IsKeyDown((int)ImGuiKey.Enter))
                {
                    if (!string.IsNullOrEmpty(OutputName))
                        hasInput = true;

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

                if (Textures != null) {

                    var width = ImGui.GetWindowWidth();

                    float size = ImGui.GetFrameHeight();
                    ImGui.BeginChild("textureList", new System.Numerics.Vector2(320, 300));
                    bool isSearch = !string.IsNullOrEmpty(_searchText);

                    foreach (var tex in Textures.OrderBy(x => x.Key))
                    {
                        bool HasText = tex.Key.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (isSearch && !HasText)
                            continue;

                        bool isSelected = OutputName == tex.Key;

                        if (tex.Value != null)
                        {
                            var sourceTex = tex.Value.OriginalSource;
                            IconManager.DrawTexture(tex.Key, sourceTex);
                            ImGui.SameLine();
                        }

                        if (!scrolled && isSelected)
                        {
                            ImGui.SetScrollHereY();
                            scrolled = true;
                        }

                        if (ImGui.Selectable(tex.Key, isSelected))
                        {
                            OutputName = tex.Key;
                            hasInput = true;
                        }
                        if (ImGui.IsItemFocused() && !isSelected)
                        {
                            OutputName = tex.Key;
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
                }
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
