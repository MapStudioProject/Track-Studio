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
            if (ImGui.Button("Randomize Double Item Boxes (Deluxe Only)"))
                RandomizeDoubleItemBoxes();
        }

        private void RandomizeDoubleItemBoxes()
        {
            Random rnd = new Random();

            foreach (var obj in Editor.Root.Children)
            {
                var objData = obj.Tag as Obj;
                if (objData.ObjId == 1013)
                {
                    //Param 1 for double item boxes
                    //0 = single. 1 = double
                    bool isDouble = rnd.Next(0, 2) != 0;

                    objData.Params[0] = isDouble ? 1 : 0;
                    Console.WriteLine($"Randomized {objData.ObjId} {objData.Params[0]} ");
                }
            }
        }
    }
}
