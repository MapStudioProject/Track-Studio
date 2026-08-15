using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using MapStudio.UI;
using ImGuiNET;
using TurboLibrary;
using GLFrameworkEngine;
using GLFrameworkEngine.UI;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Drawing;
using Toolbox.Core.IO;

namespace TurboLibrary.MuuntEditor
{
    public class MapObjectUI
    {
        static bool DisplayUnusedParams = false;
        static bool DisplayRawFloats = false;

        public void Render(Obj mapObject, IEnumerable<object> selected)
        {
            var warnings = DisplayWarnings(mapObject);
            foreach (var warning in warnings)
                ImGui.TextColored(ThemeHandler.Theme.Warning, warning);

            MapStudio.UI.ImguiBinder.LoadProperties(mapObject, selected);

            if (ImGui.CollapsingHeader(TranslationSource.GetText("PARAMETERS"), ImGuiTreeNodeFlags.DefaultOpen)) {
                LoadParameterUI(mapObject, selected);
            }
            if (ImGui.CollapsingHeader(TranslationSource.GetText("RELATIVES"), ImGuiTreeNodeFlags.DefaultOpen)) {
                DisplayLinkUI("RELATIVE_OBJECT", mapObject, "ParentObj");
                DisplayLinkUI("RELATIVE_AREA", mapObject, "ParentArea");
            }
            if (ImGui.CollapsingHeader(TranslationSource.GetText("PATHS"), ImGuiTreeNodeFlags.DefaultOpen)) {
                LoadPathUI(mapObject);
            }
        }

        private string[] DisplayWarnings(Obj mapObject)
        {
            string objectInfo = mapObject.Label;
            bool isObjPath = mapObject.ObjPath != null || (mapObject.Path != null);

            List<string> warnings = new List<string>();
            if (isObjPath && mapObject.Speed == 0)
                warnings.Add(string.Format(TranslationSource.GetText("SPEED_WARNING"), objectInfo));

            if (GlobalSettings.ObjDatabase.ContainsKey(mapObject.ObjId))
            {
                ObjDefinition objDef = GlobalSettings.ObjDatabase[mapObject.ObjId];
                if ((int)objDef.PathType == 3 && !isObjPath)
                    warnings.Add(string.Format(TranslationSource.GetText("LINK_ERROR"), objectInfo));
            }
            return warnings.ToArray();
        }

        private void LoadParameterUI(Obj mapObject, IEnumerable<object> selected)
        {
            var names = mapObject.GetParameterNames();
            MapObjMeta meta = ParamDataBaseSingleton.Instance.GetMeta(mapObject.ObjId);

            ImGui.Checkbox(TranslationSource.GetText("DISPLAY_UNUSED"), ref DisplayUnusedParams);
            ImGui.Checkbox(TranslationSource.GetText("DISPLAY_RAW"), ref DisplayRawFloats);

            float minHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2.0f;

            if (ImGui.BeginTable("params8", 2, ImGuiTableFlags.Resizable)) {
                ImGui.TableSetupColumn("params8c1", ImGuiTableColumnFlags.WidthStretch);
                //ImGui.TableSetupColumn("params8c2", ImGuiTableColumnFlags.WidthFixed, 10.0f);
                ImGui.TableSetupColumn("params8c3", ImGuiTableColumnFlags.WidthStretch);

                float rowHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2.0f;

                for (int i = 0; i < 8; i++)
                {
                    ParamDescriptor pd = meta.Params[i];
                    //Console.WriteLine($"Mapobj {mapObject.ObjId}: isDocd: {meta.IsDocumented}, isUsed {pd.IsUsed}");
                    if (!DisplayUnusedParams && !pd.IsUsed && meta.IsDocumented)
                        continue;

                    string name = pd.Name;
                    if (!meta.IsDocumented)
                        name = string.Format(TranslationSource.GetText("PARAM"), i); // TODO: Move logic to ParamDescriptor

                    var param = mapObject.Params[i];

                    ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(name);
                    ImGui.TableNextColumn();

                    string icon = $"    {IconManager.WARNING_ICON}  ";
                    if (!pd.Validate(param))
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0.15f, 1.0f));
                        ImGui.Text(icon);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 250.0f);
                            ImGui.Text($"{param:F1}f is the current value.\nIt cannot be displayed in this widget and is considered invalid.");
                            ImGui.EndTooltip();
                        }
                        ImGui.PopStyleColor();
                    }
                    else
                        ImGui.Dummy(new Vector2(ImGui.CalcTextSize(icon).X, ImGui.GetFrameHeight()));

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(-1);
                    string uiId = $"#####param{i}";

                    bool isParamChanged = false;
                    if (DisplayRawFloats || pd.Type == ParamDescriptor.ParamType.Float || pd.Type == ParamDescriptor.ParamType.UNKNOWN)
                    {
                        // Always show floats if enabled
                        isParamChanged = ImGui.InputFloat(uiId, ref param);
                    }
                    else
                    {
                        //Console.WriteLine(pd.Type);
                        switch (pd.Type)
                        {
                            case ParamDescriptor.ParamType.Int:
                                int intParam = (int)param;
                                isParamChanged = ImGui.InputInt(uiId, ref intParam, 1);
                                param = (float)intParam;
                                break;
                            case ParamDescriptor.ParamType.Frame:

                                break;
                            case ParamDescriptor.ParamType.Bool:
                                bool boolParam = param != 0f;
                                isParamChanged = ImGui.Checkbox(uiId, ref boolParam);
                                param = boolParam ? 1f : 0f;
                                break;
                            case ParamDescriptor.ParamType.Enum:

                                break;
                            case ParamDescriptor.ParamType.Bytes:
                                int bytesParam = BitConverter.SingleToInt32Bits(param);
                                isParamChanged = ImGui.InputInt(uiId, ref bytesParam, 1, 0x10, ImGuiInputTextFlags.CharsHexadecimal);
                                param = (float)BitConverter.Int32BitsToSingle(bytesParam);
                                break;
                        }
                    }

                    if (isParamChanged) {
                        foreach (Obj obj in selected)
                        {
                            obj.Params[i] = param;
                            obj.NotifyPropertyChanged("Params");
                        }
                    }
                }
                ImGui.EndTable();
            }
        }

        private void LoadPathUI(Obj mapObject)
        {
            DisplayLinkUI("OBJECT_PATH", mapObject, "ObjPath");
            if (mapObject.ObjPath != null)
                DisplayPointLinkUI("PATH_POINT", mapObject, "ObjPathPoint", mapObject.ObjPath);

            DisplayLinkUI("RAIL_PATH", mapObject, "Path");
            if (mapObject.Path != null)
                DisplayPointLinkUI("PATH_POINT", mapObject, "PathPoint", mapObject.Path);

            //Todo. The rest of these paths use groups to determine what to map to.
            //Need to find an intutive way to get these.
            DisplayGroupLinkUI<LapPath, LapPathPoint>("LAP_PATH", mapObject, "LapPath");
            if (mapObject.LapPath != null)
                DisplayLapPointLinkUI("PATH_POINT", mapObject, "LapPathPoint", mapObject.LapPath);

            DisplayGroupLinkUI<EnemyPath, EnemyPathPoint>("ENEMY_PATH_1", mapObject, "EnemyPath1");
            DisplayGroupLinkUI<EnemyPath, EnemyPathPoint>("ENEMY_PATH_2", mapObject, "EnemyPath2");
            DisplayGroupLinkUI<ItemPath, ItemPathPoint>("ITEM_PATH_1", mapObject, "ItemPath1");
            DisplayGroupLinkUI<ItemPath, ItemPathPoint>("ITEM_PATH_2", mapObject, "ItemPath2");
        }

        private void DisplayLinkUI(string text, Obj mapObject, string properyName)
        {
            EventHandler onLink = (sender, e) => {
                mapObject.NotifyPropertyChanged(properyName);
            };

            ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText(text), mapObject, properyName, onLink);
        }

        private void DisplayPointLinkUI(string text, Obj mapObject, string properyName, object path)
        {
            var pathRender = GetDrawableLink(path) as RenderablePath;
            if (pathRender == null)
                return;

            EventHandler onLink = (sender, e) => {
                mapObject.NotifyPropertyChanged(properyName);
                if (mapObject.Path != null)
                {
                    //If the path is type 3 it uses an obj path
                    //Rail types are used but baked on save as obj path
                    ObjDefinition objDef = GlobalSettings.ObjDatabase[mapObject.ObjId];
                    if ((int)objDef.PathType == 3)
                        mapObject.Path.UseAsObjPath = true;
                }
            };

            ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText(text), mapObject, properyName, pathRender.PathPoints, onLink);
        }

        private void DisplayLapPointLinkUI(string text, Obj mapObject, string properyName, object path)
        {
            EventHandler onLink = (sender, e) => {
                mapObject.NotifyPropertyChanged(properyName);
            };

            foreach (var render in GLContext.ActiveContext.Scene.Objects)
            {
                if (render is CubePathRender <LapPath, LapPathPoint>)
                {
                    var lapPathRender = render as CubePathRender<LapPath, LapPathPoint>;
                    foreach (var child in lapPathRender.NodeFolder.Children)
                    {
                        if (child.Tag == path)
                            ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText(text), mapObject, properyName, child.Children, onLink);
                    }
                }
            }
        }

        private void DisplayGroupLinkUI<TPath, TPoint>(string text, Obj mapObject, string properyName)
                where TPath : PathBase<TPath, TPoint>
                where TPoint : PathPointBase<TPath, TPoint>, new()
        {
            EventHandler onLink = (sender, e) => {
                mapObject.NotifyPropertyChanged(properyName);
            };

            foreach (var render in GLContext.ActiveContext.Scene.Objects)
            {
                if (render is PathRender<TPath, TPoint>) {
                    var path = render as PathRender<TPath, TPoint>;
                    ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText(text), mapObject, properyName, path.NodeFolder.Children, onLink);
                }
                else if (render is CubePathRender<TPath, TPoint>) {
                    var path = render as CubePathRender<TPath, TPoint>;
                    ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText(text), mapObject, properyName, path.NodeFolder.Children, onLink);
                }
            }
        }

        private IDrawable GetDrawableLink(object obj)
        {
            foreach (var render in GLContext.ActiveContext.Scene.Objects)
            {
                if (render is IRenderNode)
                {
                    var tag = ((IRenderNode)render).UINode.Tag;
                    if (tag == null || obj != tag)
                        continue;

                    return render;
                }
            }
            return null;
        }

        private bool DisplayFloat(string id, string name, ref float value)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(name);
            ImGui.NextColumn();
            bool input = ImGui.InputFloat($"###{id}", ref value);
            ImGui.NextColumn();
            return input;
        }
    }
}