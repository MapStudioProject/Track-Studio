using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using MapStudio.UI;
using UIFramework;
using ImGuiNET;

namespace CafeLibrary
{
    public class MaterialCopyWindow : Window
    {
        public bool CopyParameters = true;
        public bool CopyRenderInfo = true;
        public bool CopyOptions = true;
        public bool CopyUserData = true;

        TreeView treeView = new TreeView();
        FMAT Source;

        public MaterialCopyWindow(FMDL fmdl, FMAT source)
        {
            TreeNode modelNode = new TreeNode(fmdl.Name);
            modelNode.HasCheckBox = true;
            modelNode.Icon = IconManager.MODEL_ICON.ToString();
            modelNode.IsExpanded = true;
            modelNode.IsChecked = false;
            treeView.Nodes.Add(modelNode);

            Source = source;
            Size = new Vector2(500, 600);
            foreach (FMAT mat in fmdl.Materials)
            {
                if (Source == mat)
                    continue;

                TreeNode node = new TreeNode(mat.Name);
                node.HasCheckBox = true;
                node.IsChecked = false;
                node.Tag = mat;
                node.Icon = mat.UINode.Icon;
                node.IconColor = mat.UINode.IconColor;
                modelNode.AddChild(node);
            }
        }

        public override void Render()
        {
            ImGui.Columns(2);
            ImGui.Checkbox("Copy Parameters", ref CopyParameters);
            ImGui.Checkbox("Copy Render Info", ref CopyRenderInfo);
            ImGui.Checkbox("Copy Options/Textures/Render State", ref CopyOptions);
            ImGui.Checkbox("Copy User Data", ref CopyUserData);
            ImGui.NextColumn();

         //   ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            var posY = ImGui.GetCursorPosY();
            var csize = new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - posY - 50);
            if (ImGui.BeginChild("materialList", csize, true))
            {
                treeView.Render();
            }
            ImGui.EndChild();
          //  ImGui.PopStyleColor();

            ImGui.NextColumn();
            ImGui.Columns(1);

            var pos = ImGui.GetWindowHeight() - 31;
            ImGui.SetCursorPosY(pos);

            var size = new Vector2(ImGui.GetWindowWidth() / 2, ImGui.GetFrameHeight());
            if (ImGui.Button(TranslationSource.GetText("CANCEL"), size))
                DialogHandler.ClosePopup(false);

            ImGui.SameLine();
            if (ImGui.Button(TranslationSource.GetText("OK"), size))
                DialogHandler.ClosePopup(true);
        }

        public List<FMAT> GetDestMaterials()
        {
            List<FMAT> dests = new List<FMAT>();
            foreach (var node in treeView.Nodes[0].Children)
            {
                if (!node.IsChecked)
                    continue;

                dests.Add(node.Tag as FMAT);
            }
            return dests;
        }
    }
}
