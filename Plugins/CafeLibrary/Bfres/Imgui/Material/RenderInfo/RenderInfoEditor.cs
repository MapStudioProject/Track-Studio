using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using BfresLibrary;
using MapStudio.UI;

namespace CafeLibrary
{
    public class RenderInfoEditor
    {
        static List<RenderInfo> Selected = new List<RenderInfo>();

        static RenderInfoDialog ActiveDialog = new RenderInfoDialog();

        public static void Render(FMAT material, ResDict<RenderInfo> renderInfoDict)
        {
            if (ImGui.Button($"   {IconManager.ADD_ICON}   "))
            {
                var userData = new RenderInfo();
                userData.Name = " ";
                ShowDialog(material, renderInfoDict, userData);
            }

            var diabledTextColor = ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];
            bool isDisabledEdit = Selected.Count == 0;
            if (isDisabledEdit)
                ImGui.PushStyleColor(ImGuiCol.Text, diabledTextColor);

            ImGui.SameLine();

            bool removed = ImGui.Button($"   {IconManager.DELETE_ICON}   ") && Selected.Count > 0;

            ImGui.SameLine();
            if (ImGui.Button($"   {IconManager.EDIT_ICON}   ") && Selected.Count > 0) {
                EditRenderInfo(material, renderInfoDict, Selected[0]);
            }

            if (isDisabledEdit)
                ImGui.PopStyleColor();

            RenderHeader();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            if (ImGui.BeginChild("USER_DATA_LIST"))
            {
                int index = 0;
                foreach (var renderInfo in renderInfoDict.Values)
                {
                    bool isSelected = Selected.Contains(renderInfo);

                    ImGui.Columns(2);
                    if (ImGui.Selectable(renderInfo.Name, isSelected, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        Selected.Clear();
                        Selected.Add(renderInfo);
                    }
                    if (isSelected && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                        EditRenderInfo(material, renderInfoDict, renderInfo);

                    ImGui.NextColumn();
                    ImGui.Text(GetDataString(renderInfo, ","));
                    ImGui.NextColumn();

                    if (isSelected && ImGui.IsMouseDoubleClicked(0)) {
                        ImGui.OpenPopup("##user_data_dialog");
                    }
                    index++;

                    ImGui.Columns(1);
                }
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();

            if (removed)
            {
                foreach (var usd in Selected)
                    renderInfoDict.Remove(usd);
                Selected.Clear();
            }
        }

        static void EditRenderInfo(FMAT material, ResDict<RenderInfo> renderInfoDict, RenderInfo selected)
        {
            //Apply data to new instance (so edits can be applied after)
            var userData = new RenderInfo();
            userData.Name = selected.Name;
            if (selected.Type == RenderInfoType.Int32)  userData.SetValue(selected.GetValueInt32s());
            else if (selected.Type == RenderInfoType.Single) userData.SetValue(selected.GetValueSingles());
            else                                           userData.SetValue(selected.GetValueStrings());

            ShowDialog(material, renderInfoDict, userData);
        }

        static void ShowDialog(FMAT material, ResDict<RenderInfo> renderInfoDict, RenderInfo renderInfo)
        {
            string previousName = renderInfo.Name;

            ActiveDialog.Load(material, renderInfo);

            DialogHandler.Show("Render Info", 300, 400, () =>
            {
                ActiveDialog.Render(renderInfo);
            }, (ok) =>
            {
                if (!ok)
                    return;

                //Previous old entry
                if (previousName != renderInfo.Name && renderInfoDict.ContainsKey(previousName))
                    renderInfoDict.RemoveKey(previousName);

                //Add new entry or overrite the existing one
                if (!renderInfoDict.ContainsKey(renderInfo.Name))
                    renderInfoDict.Add(renderInfo.Name, renderInfo);
                else
                    renderInfoDict[renderInfo.Name] = renderInfo;

                Selected.Clear();
                Selected.Add(renderInfo);
            });
        }

        static void RenderHeader()
        {
            ImGui.Columns(2);
            ImGuiHelper.BoldText(TranslationSource.GetText("NAME"));
            ImGui.NextColumn();
            ImGuiHelper.BoldText(TranslationSource.GetText("VALUE"));
            ImGui.Separator();
            ImGui.Columns(1);
        }

        static string GetDataString(RenderInfo renderInfo, string seperator = "\n")
        {
            if (renderInfo.Type == RenderInfoType.Int32)
                return string.Join(seperator, renderInfo.GetValueInt32s());
            if (renderInfo.Type == RenderInfoType.Single)
                return string.Join(seperator, renderInfo.GetValueSingles());
            else
                return string.Join(seperator, renderInfo.GetValueStrings());

        }
    }
}
