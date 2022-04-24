using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using CafeLibrary;
using ImGuiNET;
using GLFrameworkEngine;
using MapStudio.UI;
using Toolbox.Core.ViewModels;

namespace TurboLibrary
{
    public class CourseIconTool8U
    {
        SarcData SarcData;
        BflimTexture MapIcon;
        GLTexture2D DisplayTexture = null;
        bool edited = false;

        public CourseIconTool8U()
        {

        }

        public void Save()
        {
            if (!edited)
                return;

            var data = SARC_Parser.PackN(SarcData);
            var comp = Toolbox.Core.IO.YAZ0.Compress(data.Item2, 3, (uint)data.Item1);
            var menuPath = GlobalSettings.GetContentPath("ui\\cmn\\menu.szs");
            if (Directory.Exists(GlobalSettings.ModOutputPath))
                 menuPath = $"{GlobalSettings.ModOutputPath}\\ui\\cmn\\menu.szs";

            File.WriteAllBytes(menuPath, comp);

            edited = false;
        }

        public void Load()
        {
            LoadMK8U();
        }

        public void Render()
        {
            if (ImGui.CollapsingHeader("UI", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (DisplayTexture == null)
                    return;

                ImGui.Image((IntPtr)DisplayTexture.ID, new Vector2(304, 256));
                if (ImGui.BeginPopupContextItem("texEdit", ImGuiPopupFlags.MouseButtonRight))
                {
                    foreach (var item in GetEditMenus())
                        MapStudio.UI.ImGuiHelper.LoadMenuItem(item);

                    ImGui.EndPopup();
                }
                var size = new Vector2(304 / 2, ImGui.GetFrameHeight() + 2);
                if (ImGui.Button(TranslationSource.GetText("EXPORT"), size))
                    MapIcon.ExportDialog();

                ImGui.SameLine();
                if (ImGui.Button(TranslationSource.GetText("REPLACE"), size))
                    MapIcon.ReplaceDialogNoConfig();
            }
        }

        private MenuItemModel[] GetEditMenus()
        {
            return new MenuItemModel[]
            {
                  new MenuItemModel("Export",   MapIcon.ExportDialog),
                  new MenuItemModel("Replace",   MapIcon.ReplaceDialogNoConfig),
            };
        }

        private void LoadMK8U()
        {
            var menu = GlobalSettings.GetContentPath("ui\\cmn\\menu.szs");
            if (File.Exists(menu))
                SarcData = SARC_Parser.UnpackRamN(Toolbox.Core.IO.YAZ0.Decompress(menu));
        }

        public void UpdateCourseIcon(string trackName)
        {
            if (SarcData == null)
                LoadMK8U();

            if (SarcData == null)
                return;

            string name = $"timg/ym_CoursePict_{trackName}_00^o.bflim";
            string hash = SARC_Parser.NameHash(name).ToString("X8");
            if (!SarcData.Files.ContainsKey(hash))
                return;

            var data = SarcData.Files[hash];
            if (DisplayTexture == null)
                DisplayTexture = GLTexture2D.CreateUncompressedTexture(4, 4, 
                    OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba8,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra);

            MapIcon = new BflimTexture(new MemoryStream(data));
            DisplayTexture.Reload((int)MapIcon.Width, (int)MapIcon.Height, MapIcon.GetDecodedSurface(0));

            MapIcon.TextureReplaced += delegate
            {
                DisplayTexture.Reload((int)MapIcon.Width, (int)MapIcon.Height, MapIcon.GetDecodedSurface(0));

                var mem = new MemoryStream();
                MapIcon.Save(mem);
                SarcData.Files[hash] = mem.ToArray();

                edited = true;
            };
        }
    }
}
