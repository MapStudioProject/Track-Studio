using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.ComponentModel;
using UIFramework;
using MapStudio.UI;
using OpenTK.Input;
using OpenTK.Graphics;
using System.IO;
using Toolbox.Core;
using GLFrameworkEngine;
using ImGuiNET;

namespace Track_Studio_Launcher
{
    public class MainWindow : UIFramework.MainWindow
    {
        /// <summary>
        /// The recently opened files.
        /// </summary>
        List<string> RecentFiles = new List<string>();
        /// <summary>
        /// The recently opened or saved project files.
        /// </summary>
        List<string> RecentProjects = new List<string>();
        //Program settings
        private GlobalSettings GlobalSettings;
        //Settings window
        private SettingsWindow SettingsWindow;
        private AboutWindow AboutWindow;
        private ProjectTab ProjectTab;
        private string AppVersion = "";

        public MainWindow()
        {
        }

        public override void OnLoad()
        {
            if (loaded)
                return;

            base.OnLoad();

            //Load global settings like language configuration
            GlobalSettings = GlobalSettings.Load();
            GlobalSettings.ReloadLanguage();
            GlobalSettings.ReloadTheme();

            AboutWindow = new AboutWindow();
            SettingsWindow = new SettingsWindow(GlobalSettings);
            ProjectTab =  new ProjectTab();
            Windows.Add(SettingsWindow);
            //Set the adjustable global font scale
            ImGui.GetIO().FontGlobalScale = GlobalSettings.Program.FontScale;
            //Init common render resources typically for debugging purposes
            RenderTools.Init();

            IconManager.TryAddIcon("TOOL_ICON", Properties.Resources.Icon);
            IconManager.TryAddIcon("DocsIcon", Properties.Resources.DocsIcon);
            IconManager.TryAddIcon("GithubIcon", Properties.Resources.GithubIcon);
            IconManager.TryAddIcon("DiscordIcon", Properties.Resources.DiscordIcon);
            IconManager.TryAddIcon("TutorialIcon", Properties.Resources.TutorialIcon);
            IconManager.TryAddIcon("SettingsIcon", Properties.Resources.SettingsIcon);

            //Share resources between contexts
            GraphicsContext.ShareContexts = true;
            //Load recent file lists
            RecentFileHandler.LoadRecentList(Path.Combine(Runtime.ExecutableDir,"Recent.txt"), RecentFiles);
            RecentFileHandler.LoadRecentList(Path.Combine(Runtime.ExecutableDir,"RecentProjects.txt"), RecentProjects);

            var asssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            AppVersion = asssemblyVersion.ToString();
        }

        public override void Render()
        {
            //base.Render();

            ImGui.BeginChild("toobar", new Vector2(75, ImGui.GetWindowHeight() - 5));
            DrawSideBar();
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("projectPanel", new Vector2(ImGui.GetWindowWidth() - 77, ImGui.GetWindowHeight() - 5));
            ProjectTab.Render();
            ImGui.EndChild();

            if (ProjectTab.CheckUpdates) {
                ProjectTab.CheckUpdates = false;
                CheckUpdates();
            }

            SettingsWindow.Opened = ProjectTab.OpenSettings;

            AboutWindow.Show();
            SettingsWindow.Show();

            DialogHandler.RenderActiveWindows();
        }

        private void CheckUpdates()
        {
            try
            {
                UpdaterHelper.Setup("MapStudioProject", "Track-Studio", "TrackStudio.exe", "Version.txt");

                var release = UpdaterHelper.TryGetLatest(Runtime.ExecutableDir, 0);
                if (release == null)
                    TinyFileDialog.MessageBoxInfoOk($"Build is up to date with the latest repo!");
                else
                {
                    int result = TinyFileDialog.MessageBoxInfoYesNo($"Found new release {release.Name}! Do you want to update?");
                    if (result == 1)
                    {
                        //Download
                        UpdaterHelper.DownloadRelease(Runtime.ExecutableDir, release, 0).Wait();
                        Console.WriteLine("Installing update..");
                        //Exit the tool and install via the updater
                        UpdaterHelper.InstallUpdate("-bl");
                    }
                }
            }
            catch (Exception ex)
            {
                TinyFileDialog.MessageBoxErrorOk($"Failed to update! {ex}. Details:\n\n {ex.StackTrace}");
            }
        }

        private void DrawSideBar()
        {
            DrawToolIcon("TOOL_ICON", $"v{AppVersion}", () => { AboutWindow.Opened = !AboutWindow.Opened; });
            DrawToolIcon("DocsIcon", "Docs", OpenDocs);
            DrawToolIcon("TutorialIcon", "Tutorial", OpenTutorial);
            DrawToolIcon("GithubIcon", "Github", OpenGithub);
            DrawToolIcon("DiscordIcon", "Discord", OpenDiscord);
        }

        private void DrawToolIcon(string icon, string text, Action action)
        {
            var id = IconManager.GetTextureIcon(icon);

            ImGui.PushStyleColor(ImGuiCol.Button, 0x000000);
            if (ImGui.ImageButton((IntPtr)id, new Vector2(55, 55)))
                action?.Invoke();
            ImGui.PopStyleColor(1);

            MapStudio.UI.ImGuiHelper.DrawCenteredText(text);
        }

        private void OpenDocs() => WebUtil.OpenURL("https://mapstudioproject.github.io/TrackStudioDocs/index.html");
        private void OpenTutorial() => WebUtil.OpenURL("https://mapstudioproject.github.io/TrackStudioDocs/tutorial/Start.html");
        private void OpenGithub() => WebUtil.OpenURL("https://github.com/MapStudioProject/Track-Studio");
        private void OpenDiscord() => WebUtil.OpenURL("https://discord.gg/TjatyEE9NW");

        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        public override void OnFocusedChanged()
        {
            base.OnFocusedChanged();
        }

        public override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }
}
