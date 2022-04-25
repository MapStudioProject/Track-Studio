using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using TurboLibrary;
using MapStudio.UI;
using Toolbox.Core;

namespace TurboLibrary.MuuntEditor
{
    public class MapObjectSelector
    {
        public int GetSelectedID() => selectedObject;
        public void SetSelectedID(int id) => selectedObject = id;

        public bool CloseOnSelect = false;

        public EventHandler SelectionChanged;

        private bool filterSkybox = false;

        private int _selectedObject = 0;
        private int selectedObject
        {
            get { return _selectedObject; }
            set
            {
                if (_selectedObject != value) {
                    _selectedObject = value;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private string _searchText = "";
        private bool isSearch = false;
        private List<ObjDefinition> objectList;
        private List<ObjDefinition> filteredObjects;

        public MapObjectSelector(List<ObjDefinition> objects)
        {
            this.objectList = objects;
        }

        public void Render(bool isDialog = true)
        {
            //A search box for filtering objects
            RenderSearchBox();
            //Track the current placement
            var posY = ImGui.GetCursorPosY();
            //Filtered or full object list
            var objects = (isSearch || filterSkybox) ? filteredObjects : objectList;

            var itemHeight = 40;
            var windowSize = ImGui.GetWindowSize();

            //Setup a child with the clip size calculations for clipping the list.
            ImGuiNative.igSetNextWindowContentSize(new System.Numerics.Vector2(0.0f, objects.Count * (itemHeight + 1)));
            ImGui.BeginChild("##object_list1", new Vector2(windowSize.X, windowSize.Y - posY - (isDialog ? 30 : 0)));
            //Draw object list
            RenderObjectList();

            ImGui.EndChild();

            //Setup cancel/ok buttons for dialog type.
            if (isDialog)
            {
                ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 202);

                bool cancel = ImGui.Button("Cancel", new Vector2(100, 23)); ImGui.SameLine();
                bool applied = ImGui.Button("Ok", new Vector2(100, 23)) && selectedObject != 0;

                if (cancel) {
                    DialogHandler.ClosePopup(false);
                }
                if (applied) {
                    DialogHandler.ClosePopup(true);
                }
            }
        }

        public void RenderSearchBox()
        {
            bool filterUpdate = false;
            if (ImGui.Checkbox(TranslationSource.GetText("FILTER_SKYBOXES"), ref filterSkybox))
                filterUpdate = true;

            //Search bar
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Search");
                ImGui.SameLine();

                var posX = ImGui.GetCursorPosX();
                var width = ImGui.GetWindowWidth();

                //Span across entire outliner width
                ImGui.PushItemWidth(width - posX);
                if (ImGui.InputText("##search_box", ref _searchText, 200))
                {
                    isSearch = !string.IsNullOrWhiteSpace(_searchText);
                    filterUpdate = true;
                }
                ImGui.PopItemWidth();
            }

            if (filterUpdate)
                filteredObjects = UpdateSearch(objectList);
        }

        public void RenderObjectList()
        {
            var objects = (isSearch || filterSkybox) ? filteredObjects : objectList;
            var itemHeight = 40;

            var clipper = new ImGuiListClipper2(objects.Count, itemHeight);
            clipper.ItemsCount = objects.Count;

            //Setup list spacing
            var spacing = ImGui.GetStyle().ItemSpacing;
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(spacing.X, 0));

            //2 columns, one for name, another for ID
            ImGui.BeginColumns("##objListColumns", 2);

            for (int line_i = clipper.DisplayStart; line_i < clipper.DisplayEnd; line_i++) // display only visible items
            {
                var mapObject = objects[line_i];
                string resName = mapObject.ResNames.FirstOrDefault();

                //Get the icon
                var icon = IconManager.GetTextureIcon("Node");
                if (IconManager.HasIcon($"{Runtime.ExecutableDir}\\Lib\\Images\\MapObjects\\{resName}.png"))
                    icon = IconManager.GetTextureIcon($"{Runtime.ExecutableDir}\\Lib\\Images\\MapObjects\\{resName}.png");

                //Load the icon onto the list
                ImGui.Image((IntPtr)icon, new Vector2(itemHeight, itemHeight)); ImGui.SameLine();
                ImGuiHelper.IncrementCursorPosX(3);

                Vector2 itemSize = new Vector2(ImGui.GetWindowWidth(), itemHeight);

                //Selection handling
                bool isSelected = selectedObject == mapObject.ObjId;
                ImGui.AlignTextToFramePadding();
                bool select = ImGui.Selectable($"{mapObject.Label}##{mapObject.ObjId}", isSelected, ImGuiSelectableFlags.SpanAllColumns, itemSize);
                bool hovered = ImGui.IsItemHovered();
                ImGui.NextColumn();

                //Display object ID
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{mapObject.ObjId}");
                ImGui.NextColumn();

                if (select)
                {
                    //Update selection
                    selectedObject = mapObject.ObjId;
                }
                if (CloseOnSelect && hovered && ImGui.IsMouseClicked(0))
                {
                    //Update selection
                    selectedObject = mapObject.ObjId;
                    DialogHandler.ClosePopup(true);
                }

                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && selectedObject != 0)
                    DialogHandler.ClosePopup(true);
            }
            ImGui.EndColumns();

            ImGui.PopStyleVar();
        }

        private List<ObjDefinition> UpdateSearch(List<ObjDefinition> objects)
        {
            List<ObjDefinition> filtered = new List<ObjDefinition>();
            for (int i = 0; i < objects.Count; i++)
            {
                if (filterSkybox && !objects[i].VR)
                    continue;

                bool HasText = objects[i].Label != null &&
                     objects[i].Label.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                HasText |= objects[i].ObjId.ToString().IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                if (isSearch && HasText || !isSearch)
                    filtered.Add(objects[i]);
            }
            return filtered;
        }
    }
}
