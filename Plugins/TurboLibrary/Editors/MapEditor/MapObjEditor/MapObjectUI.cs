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
        static bool DisplayAnyPlatform = true;

        static Vector4 UColor = new Vector4(0f, 150 / 255f, 200 / 255f, 1f);
        static Vector4 DXColor = new Vector4(230 / 255f, 0f, 18 / 255f, 1f);
        static Vector4 ModColor = new Vector4(1f, 90 / 255f, 37 / 255f, 1f);

        public void Render(Obj mapObject, IEnumerable<object> selected)
        {
            var warnings = DisplayWarnings(mapObject);
            foreach (var warning in warnings)
                ImGui.TextColored(ThemeHandler.Theme.Warning, warning);

            MapStudio.UI.ImguiBinder.LoadProperties(mapObject, selected);

            if (ImGui.CollapsingHeader($"MapObject - {mapObject.Meta.Name}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                LoadMetaUI(mapObject, selected);
            }
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

        private void LoadMetaUI(Obj mapObject, IEnumerable<object> selected)
        {
            ImGui.Indent(5f);
            MapObjMeta meta = mapObject.Meta;
            ImGui.Spacing();
            DrawPlatformBadges(meta.Platforms);
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - 25f);
            if(ImGui.Button($"  {IconManager.EDIT_ICON}  ##metaEdit"))
                FileUtility.OpenFolder(GlobalSettings.ParamDataBase.GetUserArchivesPath());
                       
            ImGui.Separator();
            ImGui.PushTextWrapPos(ImGui.GetWindowContentRegionMax().X - 5f);
            ImGui.TextWrapped(meta.Description);

            ImGui.Spacing();
            if (meta.Usages.Count > 0)
                ImGuiHelper.BoldTextLabel(TranslationSource.GetText("TRACK_USAGE"), string.Join("  |  ", meta.Usages.Select(s => $"{s}")));

            if (meta.Aliases.Count > 0)
            {
                ImGui.Spacing();
                string title = TranslationSource.GetText("TAGS") + ":";
                ImGuiHelper.BoldText(title);
                float lineSize = ImGui.CalcTextSize(title).X + 5f;
                foreach (var alias in meta.Aliases)
                {
                    // Wrap badge if it doesn't fit on the same line.
                    float size = ImGui.GetStyle().ItemSpacing.X + getBadgeSize(alias).X;
                    if (ImGui.GetContentRegionAvail().X >= lineSize + size)
                        ImGui.SameLine();
                    else
                        lineSize = 0;
                    lineSize += size;
                    DrawBadge(alias);
                }
            }
            ImGui.Unindent(5f);
            ImGui.Spacing();
        }

        private void LoadParameterUI(Obj mapObject, IEnumerable<object> selected)
        {
            MapObjMeta meta = mapObject.Meta;

            ImGui.AlignTextToFramePadding();
            ImGuiHelper.BoldText(TranslationSource.GetText("DISPLAY"));
            ImGui.SameLine(0, 10f);
            ImGui.Checkbox(TranslationSource.GetText("DISPLAY_UNUSED"), ref DisplayUnusedParams);
            ImGui.SameLine(0, 10f);
            ImGui.Checkbox(TranslationSource.GetText("DISPLAY_RAW"), ref DisplayRawFloats);
            ImGui.SameLine(0, 10f);
            ImGui.Checkbox(TranslationSource.GetText("DISPLAY_ANY_PLATFORM"), ref DisplayAnyPlatform);

            float minHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2.0f;

            if (ImGui.BeginTable("params8", 3, ImGuiTableFlags.Resizable)) {
                ImGui.TableSetupColumn("params8c1", ImGuiTableColumnFlags.WidthStretch, 1f);
                ImGui.TableSetupColumn("params8c2", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.NoResize, 1f);
                ImGui.TableSetupColumn("params8c3", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 25f);

                float rowHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2.0f;

                for (int i = 0; i < 8; i++)
                {
                    string uiId = $"##param{i}";
                    ParamDescriptor pd = meta.Params[i];
                    // Hide unused parameters
                    if (!DisplayUnusedParams && !pd.IsUsed && meta.IsDocumented)
                        continue;

                    // Hide parameters not supported by the current version
                    if (!DisplayAnyPlatform && (GlobalSettings.IsMK8D ? pd.Platforms.VersionDX == DXVersion.None : pd.Platforms.VersionU == UVersion.None))
                        continue;

                    var param = mapObject.Params[i];

                    ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);

                    ImGui.TableNextColumn();
                    // Name
                    ImGui.AlignTextToFramePadding();
                    DisplayParamInfo(uiId, pd, meta, param);
                    ImGui.SameLine();
                    ImGui.TextWrapped(pd.Name);

                    ImGui.TableNextColumn();
                    // Inputs
                    bool isParamChanged = false;
                    if (DisplayRawFloats || pd.Type == ParamDescriptor.ParamType.Float || pd.Type == ParamDescriptor.ParamType.UNKNOWN)
                    {
                        // Always show floats if enabled
                        ImGui.SetNextItemWidth(-1);
                        isParamChanged = ImGui.InputFloat(uiId, ref param);
                    }
                    else
                    {
                        switch (pd.Type)
                        {
                            case ParamDescriptor.ParamType.Int:
                                int intParam = (int)param;
                                ImGui.SetNextItemWidth(-1);
                                isParamChanged = ImGui.InputInt(uiId, ref intParam, 1);
                                param = (float)intParam;
                                break;
                            case ParamDescriptor.ParamType.Time:
                                int frameParam = (int)param;
                                isParamChanged = DisplayTimer(uiId, ref frameParam);
                                param = (float)frameParam;
                                break;
                            case ParamDescriptor.ParamType.Bool:
                                bool boolParam = param != 0f;
                                ImGui.SetNextItemWidth(-1);
                                isParamChanged = ImGui.Checkbox(uiId, ref boolParam);
                                param = boolParam ? 1f : 0f;
                                break;
                            case ParamDescriptor.ParamType.Enum:
                                string selectedS = pd.Enum.ContainsKey(param) ? $"{pd.Enum[param]} ({param})" : $"{TranslationSource.GetText("ENUM_UNKNOWN")} ({param})";
                                ImGui.SetNextItemWidth(-1);
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
                                ImGui.SetNextItemWidth(-1);
                                isParamChanged = ImGui.InputInt(uiId, ref bytesParam, 1, 0x10, ImGuiInputTextFlags.CharsHexadecimal);
                                param = (float)BitConverter.Int32BitsToSingle(bytesParam);
                                break;
                        }
                    }

                    // reset to default
                    ImGui.TableNextColumn();
                    if (ImGui.Button($"  {IconManager.RESET_ICON}  ##{uiId}"))
                    {
                        isParamChanged = true;
                        param = pd.Default;
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

        private static readonly Vector2 BadgePadding = new Vector2(5f, 1f);
        private Vector2 getBadgeSize(string label)
        {
            return ImGui.CalcTextSize(label) + BadgePadding * 2f;
        }
              
        /// <summary>
        /// Draws a badge with some label
        /// </summary>
        /// <param name="label"></param>
        private void DrawBadge(string label, Vector4? bgColor = null, float rounding = 0.4f)
        {
            Vector4 _bgColor = bgColor ?? new Vector4(0.2f, 0.3f, 0.4f, 1.0f);
            Vector2 size = getBadgeSize(label);
            Vector2 pos = ImGui.GetCursorScreenPos();
            uint background = ImGui.GetColorU32(_bgColor);

            ImGui.GetWindowDrawList().AddRectFilled(pos, pos + size, background, size.Y * rounding);

            ImGui.SetCursorScreenPos(pos + BadgePadding);
            ImGui.Text(label);

            ImGui.SetCursorScreenPos(pos);
            ImGui.Dummy(size);
        }

        private void DrawPlatformBadges(MetaPlatforms p)
        {
            List<Tuple<string, Vector4?>> badges = [];
            var versionU = p.VersionU;
            var versionDX = p.VersionDX;
            if (versionU >= UVersion.Base)
                badges.Add(Tuple.Create($"   {IconManager.ICON_WII_U}    {TranslationSource.GetText("PLATFORM_U")}", (Vector4?)UColor));
            if (versionU > UVersion.Base && versionU < UVersion.Modded)
                badges.Add(Tuple.Create($"   {IconManager.ICON_DOWNLOAD}    {TranslationSource.GetText($"DLC_U_{versionU.ToString().ToUpper()}")}", (Vector4?)null));
            if (versionU == UVersion.Modded)
                badges.Add(Tuple.Create($"   {IconManager.ICON_MOD}    {TranslationSource.GetText("PLATFORM_MOD")}", (Vector4?)ModColor));
            if (versionDX >= DXVersion.Base)
                badges.Add(Tuple.Create($"   {IconManager.ICON_SWITCH}    {TranslationSource.GetText("PLATFORM_SWITCH")}", (Vector4?)DXColor));
            if (versionDX > DXVersion.Base && versionDX < DXVersion.Modded)
                badges.Add(Tuple.Create($"   {IconManager.ICON_DOWNLOAD}    {TranslationSource.GetText($"DLC_DX_{versionDX.ToString().ToUpper()}")}", (Vector4?)null));
            if (versionDX == DXVersion.Modded)
                badges.Add(Tuple.Create($"   {IconManager.ICON_MOD}    {TranslationSource.GetText("PLATFORM_MOD")}", (Vector4?)ModColor));

            bool isFirst = true;
            foreach (Tuple<string, Vector4?> b in badges)
            {
                if (!isFirst)
                    ImGui.SameLine();
                isFirst = false;
                DrawBadge(b.Item1, b.Item2);
            }
        }

        /// <summary>
        /// Displays the meta information of a given parameter
        /// </summary>
        private void DisplayParamInfo(string uiId, ParamDescriptor pd, MapObjMeta meta, float param)
        {
            bool isValid = pd.Validate(param);
            string buttonText = $"   {(isValid ? IconManager.INFO_ICON : IconManager.WARNING_ICON)}  ";
            if (!pd.IsUsed || !meta.IsDocumented)
            {
                // Skip drawing info if the parameter is unused, or we didn't know the object
                ImGui.Dummy(new Vector2(ImGui.CalcTextSize(buttonText).X, ImGui.GetFrameHeight()));
                return;
            }
            
            // Determine icon color based on platform/modded status
            Vector4 color = ThemeHandler.Theme.Text;
            var uVersion = pd.Platforms.VersionU;
            var dxVersion = pd.Platforms.VersionDX;

            if (!isValid)
                color = ThemeHandler.Theme.Warning;
            else if (GlobalSettings.IsMK8D ? dxVersion == DXVersion.Modded : uVersion == UVersion.Modded)
                color = ModColor;
            else if (dxVersion == DXVersion.None && uVersion != UVersion.None)
                color = UColor;
            else if (dxVersion != DXVersion.None && uVersion == UVersion.None)
                color = DXColor;

            ImGui.TextColored(color, buttonText);
            if (ImGui.IsItemHovered())
            {
                ImGui.AlignTextToFramePadding();
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(6f, 6f));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                ImGui.BeginTooltip();

                // Name and type
                ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 500f);
                DrawBadge(pd.Type == ParamDescriptor.ParamType.UNKNOWN ? "???" : pd.Type.ToString(), null, 0.1f);
                ImGui.SameLine(0, 5f);
                ImGuiHelper.BoldText(pd.Name);
                ImGui.Separator();

                // Display platforms (U, DX, mod)
                DrawPlatformBadges(pd.Platforms);

                // Description
                if (!string.IsNullOrWhiteSpace(pd.Description))
                    ImGui.TextWrapped(pd.Description);
                

                // Display default/min/max
                ImGuiHelper.BoldTextLabel(TranslationSource.GetText("PARAM_DEFAULT"), $"{pd.Default:0.0##}");
                if (pd.MinValue is not null)
                {
                    ImGui.SameLine();
                    ImGuiHelper.BoldTextLabel(TranslationSource.GetText("PARAM_MIN"), $"{pd.MinValue:0.0##}");
                }
                if (pd.MaxValue is not null)
                {
                    ImGui.SameLine();
                    ImGuiHelper.BoldTextLabel(TranslationSource.GetText("PARAM_MAX"), $"{pd.MaxValue:0.0##}");
                }

                // Example values
                if (pd.Samples.Count > 0)
                    ImGuiHelper.BoldTextLabel(TranslationSource.GetText("PARAM_SAMPLE"), string.Join("  |  ", pd.Samples.Select(s => $"{s:0.###}")));

                // Invalid
                if (!isValid)
                    ImGui.TextColored(ThemeHandler.Theme.Warning, string.Format(TranslationSource.GetText("PARAM_WARNING"), $"{param:0.0##}"));
                
                ImGui.PopStyleVar();
                ImGui.PopStyleColor();
                ImGui.EndTooltip();
            }

        }
    }
}