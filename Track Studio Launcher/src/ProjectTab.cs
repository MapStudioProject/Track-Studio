using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using MapStudio.UI;
using ImGuiNET;

namespace Track_Studio_Launcher
{
    internal class ProjectTab
    {
        public bool OpenSettings = false;

        public bool CheckUpdates = false;

        private string SelectedProject = "";
        private bool isSearch => !string.IsNullOrEmpty(_searchText);
        private string _searchText = "";

        private Dictionary<string, ProjectFile> Projects = new Dictionary<string, ProjectFile>();
        private List<string> FilteredProjects = new List<string>();

        public ProjectTab() {
            Reload();
        }

        public void Render()
        {
            if (ImGui.BeginTabBar("projectTab"))
            {
                if (ImguiCustomWidgets.BeginTab("projectTab", "Projects"))
                {
                    DrawTopMenu();
                    DrawProjectList();
                    ImGui.EndTabItem();
                }
                if (ImguiCustomWidgets.BeginTab("projectTab", "Templates"))
                {
                    DrawTopMenu();

                    ImGuiHelper.BoldText("Coming Soon!");
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }

        private void Reload() {
            var dirs = Directory.GetDirectories(GlobalSettings.Current.Program.ProjectDirectory).ToList();

            Projects.Clear();
            foreach (var project in dirs)
            {
                if (!File.Exists(Path.Combine(project,"Project.json")))
                    continue;

                bool HasText = new DirectoryInfo(project).Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                if (isSearch && !HasText)
                    continue;

                Projects.Add(project, ProjectFile.Load(Path.Combine(project,"Project.json")));
            }

            var sorted = Projects.ToList();
            sorted.Sort((x, y) => DateTime.Compare(y.Value.DateTime, x.Value.DateTime));

            Projects = sorted.ToDictionary(x => x.Key, x=> x.Value);
        }

        private void DrawTopMenu()
        {
            var size = new Vector2(130, 28);

            ImGui.Button($"   {IconManager.ADD_ICON}    Create Project", size);
            ImGui.SameLine();

            ImGui.Button($"   {IconManager.EDIT_ICON}    Open Project", size);
            ImGui.SameLine();

            ImGui.Button($"   {'\uf279'}    Open Course", size);
            ImGui.SameLine();

            if (ImGui.Button($"   {IconManager.SETTINGS_ICON}    Settings", size))
                OpenSettings = !OpenSettings;

            ImGui.SameLine();

            if (ImGui.Button($"   {'\uf063'}    Check Updates", size))
                CheckUpdates = true;
        }

        private void DrawProjectList()
        {
            ImGui.BeginChild("projectChild");

            /*  ImGui.Text($"Game:"); ImGui.SameLine();
            ImGui.PushItemWidth(150);
             if (ImGui.BeginCombo("##Game", "Mario Kart 8 U"))
             {
                 ImGui.EndCombo();
             }
             ImGui.SameLine();
             ImGui.Text($"Sort:"); ImGui.SameLine();
             if (ImGui.BeginCombo("##Sort", "Recent"))
             {
                 ImGui.EndCombo();
             }
             ImGui.PopItemWidth();
             */

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"   {IconManager.SEARCH_ICON}   ");
            ImGui.SameLine();
            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 25);

            if (ImGui.InputText("##searchBox", ref _searchText, 0x100))
            {
                Reload();
            }

            ImGui.PopItemWidth();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            var listSize = new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - 75);
            ImGui.BeginChild("projectList", listSize);

            var projects = Projects;
            foreach (var dir in projects)
            {
                DisplayProject(dir.Key, dir.Value);
            }

            ImGui.EndChild();
            ImGui.PopStyleColor();

            ImGuiHelper.BoldText("Project Directory:");

            ImGui.PushItemWidth(ImGui.GetWindowWidth() - 2);
            ImguiCustomWidgets.PathSelector("##ProjectDirectory", ref GlobalSettings.Current.Program.ProjectDirectory);
            ImGui.PopItemWidth();

            ImGui.EndChild();
        }

        private void DisplayProject(string folder, ProjectFile projectFile)
        {
            string thumbFile = Path.Combine(folder,"Thumbnail.png");
            string projectName = new DirectoryInfo(folder).Name;
            int height = 80;

            var icon = IconManager.GetTextureIcon("BLANK");
            if (File.Exists(thumbFile))
            {
                IconManager.LoadTextureFile(thumbFile, 1024, 1024);
                icon = IconManager.GetTextureIcon(thumbFile);
            }

            //Make the whole menu item region selectable
            var item_size = 150;

            var size = new Vector2(ImGui.GetWindowWidth(), height);

            bool isSelected = SelectedProject == folder;

            var pos = ImGui.GetCursorPos();
            if (ImGui.Selectable($"##{folder}", isSelected, ImGuiSelectableFlags.None, size))
                SelectedProject = folder;

            var endpos = ImGui.GetCursorPos();

            var doubleClicked = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0);

            ImGui.SetCursorPos(pos);

            //Load an icon preview of the project
            ImGui.AlignTextToFramePadding();
            ImGui.Image((IntPtr)icon, new Vector2(item_size, height),
                new Vector2(0, 0.25f),
                new Vector2(1, 0.75f));
            //Project name

            var textpos = ImGui.GetCursorPos();

            var p = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(p.X, p.Y),
                new Vector2(p.X + item_size, p.Y - 18),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.3f, 0.9f)));

            ImGui.SetCursorPos(new Vector2(pos.X + 5, pos.Y + height - 14));
            ImGui.Text(projectName);

            ImGui.SameLine();

            ImGui.SetCursorPos(new Vector2(pos.X + item_size + 5, pos.Y + height - 14));

            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), $"Editor: {projectFile.ActiveEditor}");

            ImGui.SetCursorPos(new Vector2(pos.X + item_size + 5, pos.Y + height - 40));
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), $"Date: {projectFile.SaveDate}");


            ImGui.SetCursorPos(endpos);

            //Load file when clicked on
            if (doubleClicked)
            {
            }
        }
    }
}
