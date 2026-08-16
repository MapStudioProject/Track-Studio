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
using System.Collections;

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
            ImGui.SameLine(0, 10f);
            ImGui.Checkbox(TranslationSource.GetText("DISPLAY_RAW"), ref DisplayRawFloats);

            float minHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2.0f;

            if (ImGui.BeginTable("params8", 2, ImGuiTableFlags.Resizable)) {
                ImGui.TableSetupColumn("params8c1", ImGuiTableColumnFlags.WidthStretch);
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
                            ImGui.Text($"{param:0.0##}f is the current value.\nIt cannot be displayed in this widget and is considered invalid.");
                            ImGui.EndTooltip();
                        }
                        ImGui.PopStyleColor();
                    }
                    else
                        ImGui.Dummy(new Vector2(ImGui.CalcTextSize(icon).X, ImGui.GetFrameHeight()));

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(-1);
                    string uiId = $"##param{i}";

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
                                int frameParam = (int)param;
                                isParamChanged = DisplayTimer(uiId, ref frameParam);
                                param = (float)frameParam;
                                break;
                            case ParamDescriptor.ParamType.Bool:
                                bool boolParam = param != 0f;
                                isParamChanged = ImGui.Checkbox(uiId, ref boolParam);
                                param = boolParam ? 1f : 0f;
                                break;
                            case ParamDescriptor.ParamType.Enum:
                                string selectedS = pd.Enum.ContainsKey(param) ? $"{pd.Enum[param]} ({param})" : $"{TranslationSource.GetText("UNKNOWN")} ({param})";
                                if (ImGui.BeginCombo(uiId, selectedS)) {
                                    foreach (KeyValuePair<float, string> e in pd.Enum)
                                    {
                                        bool isSelected = param == e.Key;

                                        if (ImGui.Selectable($"{e.Value} ({e.Key})", isSelected))
                                        {
                                            param = e.Key;
                                            isParamChanged = true;
                                        }

                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                        
                                    }
                                    ImGui.EndCombo();
                                }
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

        /// <summary>
        /// Displays a m:ss.mmm timer widget, based on a given number of frames
        /// </summary>
        /// <param name="id">id used for UI components</param>
        /// <param name="frames">Number of frames to display</param>
        /// <returns></returns>
        private bool DisplayTimer(string id, ref int frames)
        {
            // 60 FPS -> approximately 16.667 ms per frame.
            const int FPS = 60;
            const int FPM = FPS * 60;

            // Convert frames to m:ss.mmm
            int minutes = frames / FPM;
            int seconds = (frames / FPS) % 60;
            int remainingFrames = frames % FPS;
            int milliseconds = (int)(remainingFrames * 1000.0 / FPS);

            bool changed = false;
            var flags = ImGuiInputTextFlags.CharsDecimal | ImGuiInputTextFlags.AutoSelectAll;

            string minutesS = minutes.ToString();
            string secondsS = seconds.ToString("00");
            string millisecondsS = milliseconds.ToString("000");

            ImGui.SetNextItemWidth(16);
            if (ImGui.InputText($"{id}_minutes", ref minutesS, 1, flags))
            {
                int.TryParse(minutesS, out minutes);
                minutes = Math.Max(0, minutes);
                changed = true;
            }

            ImGui.SameLine(0, 2);
            ImGui.Text(":");
            ImGui.SameLine(0, 2);

            ImGui.SetNextItemWidth(24);
            if (ImGui.InputText($"{id}_seconds", ref secondsS, 2, flags))
            {
                int.TryParse(secondsS, out seconds);
                seconds = Math.Max(0, Math.Min(59, seconds));
                changed = true;
            }

            ImGui.SameLine(0, 2);
            ImGui.Text(".");
            ImGui.SameLine(0, 2);

            ImGui.SetNextItemWidth(32);
            if (ImGui.InputText($"{id}_milliseconds", ref millisecondsS, 3, flags))
            {
                int.TryParse(millisecondsS, out milliseconds);
                milliseconds = Math.Max(0, Math.Min(999, milliseconds));
                changed = true;
            }

            if (changed)
            {
                // Convert the edited m:ss.mmm back to frames.
                frames =
                    minutes * FPM +
                    seconds * FPS +
                    (int)(milliseconds * FPS / 1000.0);
            }

            return changed;
        }
    }
}