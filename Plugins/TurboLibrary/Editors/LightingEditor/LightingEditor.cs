using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using GLFrameworkEngine;
using UIFramework;

namespace TurboLibrary.LightingEditor
{
    public class LightingEditorWindow : DockWindow
    {
        public override string Name => TranslationSource.GetText("LIGHTING_EDITOR");

        public ColorCorrectionWindow ColorCorrectionWindow;
        public CubemapUintWindow CubemapUintWindow;
        public LightMapEditor LightMapEditor;
        public EnvironmentEditor EnvironmentEditor;

        public override ImGuiWindowFlags Flags => ImGuiWindowFlags.MenuBar;

        CourseMuuntPlugin Plugin;

        public LightingEditorWindow(DockSpaceWindow dockSpace, CourseMuuntPlugin plugin) : base(dockSpace)
        {
            Plugin = plugin;
            ColorCorrectionWindow = new ColorCorrectionWindow();
            CubemapUintWindow = new CubemapUintWindow();
            LightMapEditor = new LightMapEditor();
            EnvironmentEditor = new EnvironmentEditor();
            Size = new System.Numerics.Vector2(500, 700);
        }

        public override void Render()
        {
            var context = GLContext.ActiveContext;

            bool propertyChanged = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Open"))
                {
                }
                if (ImGui.MenuItem("Save"))
                {
                    Plugin.MapLoader.SaveLightingResources();
                }

                ImGui.EndMenuBar();
            }

            ImGui.BeginTabBar("Menu1");

            if (ImguiCustomWidgets.BeginTab("Menu1", "Color Correction"))
            {
                propertyChanged |= ColorCorrectionWindow.Render(context);
                ImGui.EndTabItem();
            }

            if (ImguiCustomWidgets.BeginTab("Menu1", "Course Environment"))
            {
                propertyChanged |= EnvironmentEditor.Render(context);
                ImGui.EndTabItem();
            }

            //Todo
            //Light maps just consist of course env data so it isn't too important for now.
          /*  if (ImguiCustomWidgets.BeginTab("Menu1", "Light Maps"))
            {
                LightMapEditor.Render(context);
                ImGui.EndTabItem();
            }*/

            if (ImguiCustomWidgets.BeginTab("Menu1", "Cube Maps"))
            {
                CubemapUintWindow.Render(context);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}
