using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapStudio.UI;
using ImGuiNET;

namespace TurboLibrary.MuuntEditor
{
    public class MapObjectToolMenu : IToolWindowDrawer
    {
        ObjectEditor Editor;

        public MapObjectToolMenu(ObjectEditor editor) {
            Editor = editor;
        }

        public void Render()
        {
         /*   var toolMenus = Editor.GetToolMenuItems();
            var editMenus = Editor.GetEditMenuItems();

            var size = new System.Numerics.Vector2(150, 22);*/

         /*   if (toolMenus.Count > 0 && ImGui.CollapsingHeader("Tools", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var menu in toolMenus)
                {
                    if (ImGui.Button(menu.Header, size)) {
                        menu.Command.Execute(menu);
                    }
                }
            }
            if (editMenus.Count > 0 && ImGui.CollapsingHeader("Actions", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var menu in editMenus)
                {
                    if (ImGui.Button(menu.Header, size)) {
                        menu.Command.Execute(menu);
                    }
                }
            }*/
        }
    }
}
