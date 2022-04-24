using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapStudio.UI;
using UIFramework;
using BfresLibrary;
using CafeLibrary.Rendering;
using Toolbox.Core.Animations;
using ImGuiNET;

namespace CafeLibrary
{
    class MaterialAnimUI
    {
        public static TreeNode ReloadTree(TreeNode Root, BfresMaterialAnim anim, ResFile resFile)
        {
            if (resFile == null)
                return null;

            Root.Children.Clear();
            Root.Header = anim.Name;
            Root.CanRename = true;
            Root.OnHeaderRenamed += delegate
            {
                string previousName = anim.UINode.Header;

                anim.UINode.Header = Root.Header;
                anim.OnNameChanged(anim.UINode.Header);
            };
            Root.Tag = anim;
            Root.Icon = anim.UINode.Icon;
            Root.IsExpanded = true;
            Root.ContextMenus.Add(new MenuItem("Add Material", () =>
            {
                var matGroup = new BfresMaterialAnim.MaterialAnimGroup();
                matGroup.Name = "NewMaterial";
                anim.AnimGroups.Add(matGroup);
                //Add to ui
                Root.AddChild(GetGroupNode(anim, null, matGroup));
                anim.IsEdited = true;
            }));

            Root.ContextMenus.Add(new MenuItem("Rename", () => Root.ActivateRename = true));
            foreach (var group in anim.AnimGroups)
            {
                Material material = null;
                foreach (var model in anim.ResFile.Models.Values)
                {
                    if (model.Materials.ContainsKey(group.Name))
                        material = model.Materials[group.Name];
                }
                Root.AddChild(GetGroupNode(anim, material, group));
            }
            return Root;
        }

        public static TreeNode GetGroupNode(BfresMaterialAnim anim, Material material, STAnimGroup group)
        {
            TreeNode matNode = new TreeNode(group.Name);
            matNode.IsExpanded = true;
            matNode.Tag = group;
            matNode.Icon = '\uf5fd'.ToString();
            matNode.CanRename = true;
            matNode.OnHeaderRenamed += delegate
            {
                //Update bfres data
                foreach (var mat in anim.MaterialAnim.MaterialAnimDataList)
                {
                    if (mat.Name == group.Name)
                        mat.Name = matNode.Header;
                }
                group.Name = matNode.Header;
            };
            matNode.ContextMenus.Add(new MenuItem("Add Texture Anim", () =>
            {
                var track = new BfresMaterialAnim.SamplerTrack();
                track.Name = "_a0";
                track.InterpolationType = STInterpoaltionType.Step;
                //Add to anim
                ((BfresMaterialAnim.MaterialAnimGroup)group).Tracks.Add(track);
                //Add to UI
                matNode.AddChild(CreateSamplerNodeHierachy(anim, track, group));
            }));
            matNode.ContextMenus.Add(new MenuItem("Add Shader Param Anim", () =>
            {
                var targetGroup = new BfresMaterialAnim.ParamAnimGroup();
                ShaderParam targetParam = new ShaderParam();
                targetParam.Name = "";
                targetParam.Type = ShaderParamType.TexSrt;

                DialogHandler.Show("Add Param Anim", 400, 120,  () =>
                {
                    Material material = null;
                    foreach (var model in anim.ResFile.Models.Values)
                    {
                        if (model.Materials.ContainsKey(group.Name))
                            material = model.Materials[group.Name];
                        else
                            material = model.Materials.Values.FirstOrDefault();
                    }

                    if (material != null)
                    {
                        if (ImGui.BeginCombo("Param", targetParam.Name))
                        {
                            foreach (var param in material.ShaderParams.Values)
                            {
                                bool selected = param == targetParam;
                                if (ImGui.Selectable(param.Name, selected))
                                    targetParam = param;

                                if (selected)
                                    ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndCombo();
                        }
                    }
                    else
                    {
                        string name = targetParam.Name;
                        if (ImGui.InputText("Name", ref name, 0x200))
                            targetParam.Name = name;
                        if (MapStudio.UI.ImGuiHelper.ComboFromEnum<ShaderParamType>("Type", targetParam, "Type"))
                        {
                            if (targetParam.Type == ShaderParamType.Float) targetParam.DataValue = 0;
                            else if (targetParam.Type == ShaderParamType.Float2) targetParam.DataValue = new float[2];
                            else if (targetParam.Type == ShaderParamType.Float3) targetParam.DataValue = new float[3];
                            else if (targetParam.Type == ShaderParamType.Float4) targetParam.DataValue = new float[4];
                            else if (targetParam.Type == ShaderParamType.TexSrt ||
                                     targetParam.Type == ShaderParamType.TexSrtEx)
                            {
                                targetParam.DataValue = new TexSrt()
                                {
                                    Scaling = new Syroot.Maths.Vector2F(1, 1),
                                    Rotation = 0,
                                    Translation = new Syroot.Maths.Vector2F(),
                                    Mode = TexSrtMode.ModeMaya,
                                };
                            }
                            else if (targetParam.Type == ShaderParamType.Int2) targetParam.DataValue = new int[2];
                            else if (targetParam.Type == ShaderParamType.Int3) targetParam.DataValue = new int[3];
                            else if (targetParam.Type == ShaderParamType.Int3) targetParam.DataValue = new int[4];
                            else if (targetParam.Type == ShaderParamType.UInt2) targetParam.DataValue = new int[2];
                            else if (targetParam.Type == ShaderParamType.UInt3) targetParam.DataValue = new int[3];
                            else if (targetParam.Type == ShaderParamType.UInt4) targetParam.DataValue = new int[4];
                        }
                    }

                    ImGui.SetCursorPosY(ImGui.GetWindowHeight() - 30);
                    if (ImGui.Button("Apply", new System.Numerics.Vector2(ImGui.GetWindowWidth(), 23))) {
                        if (string.IsNullOrEmpty(targetParam.Name))
                            return;

                        DialogHandler.ClosePopup(true);
                    }

                }, (ok) =>
                {
                    if (ok)
                    {
                        targetGroup.Name = targetParam.Name;
                        matNode.AddChild(CreateParamNodeHierachy(anim, targetParam, targetGroup, group));
                    }
                });
            }));

            matNode.ContextMenus.Add(new MenuItem("Rename", () => matNode.ActivateRename = true));
            matNode.ContextMenus.Add(new MenuItem(""));
            matNode.ContextMenus.Add(new MenuItem("Duplicate", () =>
            {
                var parent = matNode.Parent;

                var matGroup = ((BfresMaterialAnim.MaterialAnimGroup)group).Clone();
                anim.AnimGroups.Add(matGroup);
                //Add to ui
                parent.AddChild(GetGroupNode(anim, null, matGroup));
                anim.IsEdited = true;
            }));

            matNode.ContextMenus.Add(new MenuItem("Delete", () =>
            {
                foreach (var child in matNode.Children)
                {
                    if (child is AnimationTree.GroupNode)
                        ((AnimationTree.GroupNode)child).OnGroupRemoved?.Invoke(child, EventArgs.Empty);
                }

                //Remove from animation
                anim.AnimGroups.Remove(group);
                //Remove from UI
                anim.Root.Children.Remove(matNode);
                anim.IsEdited = true;
            }));

            ShaderParam GetParam(string name)
            {
                if (material != null && material.ShaderParams.ContainsKey(name))
                    return material.ShaderParams[name];

                return null;
            }

            foreach (STAnimGroup targetGroup in group.SubAnimGroups)
            {
                if (targetGroup is BfresMaterialAnim.ParamAnimGroup)
                    matNode.AddChild(CreateParamNodeHierachy(anim, GetParam(targetGroup.Name), (BfresMaterialAnim.ParamAnimGroup)targetGroup, group));
            }
            foreach (var track in group.GetTracks())
            {
                if (track is BfresMaterialAnim.SamplerTrack)
                    matNode.AddChild(CreateSamplerNodeHierachy(anim, (BfresMaterialAnim.SamplerTrack)track, group));
            }

            return matNode;
        }

        public static TreeNode CreateSamplerNodeHierachy(BfresMaterialAnim anim, BfresMaterialAnim.SamplerTrack track, STAnimGroup matGroup)
        {
            var trackNode = new SamplerTreeTrack(anim, track, matGroup);
            trackNode.CanRename = true;
            trackNode.ContextMenus.Add(new MenuItem("Rename", () => trackNode.ActivateRename = true));
            trackNode.ContextMenus.Add(new MenuItem(""));
            trackNode.ContextMenus.Add(new MenuItem("Delete", () =>
            {
                var parent = trackNode.Parent;

                //Remove from animation
                ((BfresMaterialAnim.MaterialAnimGroup)matGroup).Tracks.Remove(trackNode.Track);
                //Remove from UI
                parent.Children.Remove(trackNode);
                anim.IsEdited = true;
            }));

            return trackNode;
        }

        public static TreeNode CreateParamNodeHierachy(BfresMaterialAnim anim, ShaderParam param, BfresMaterialAnim.ParamAnimGroup group, STAnimGroup matGroup)
        {
            AnimationTree.GroupNode paramNode = new AnimationTree.GroupNode(anim, group, matGroup);
            paramNode.Icon = '\uf6ff'.ToString();
            if (group.Name.Contains("color"))
            {
                paramNode = new ColorTreeNode(anim, group, matGroup);
                paramNode.Icon = '\uf53f'.ToString();
            }

            paramNode.OnGroupRemoved += delegate
            {
                var materials = anim.GetMaterials();
                //Remove animation from materials
                if (materials != null)
                {
                    foreach (var mat in materials.Where(x => x.Name == matGroup.Name))
                    {
                        //Remove from animation
                        if (mat.Material.AnimatedParams.ContainsKey(group.Name))
                        {
                            mat.Material.AnimatedParams.Remove(group.Name);
                            mat.Material.OnParamUpdated(mat.Material.ShaderParams[group.Name]);
                        }
                    }
                }
            };

            paramNode.CanRename = true;
            paramNode.OnHeaderRenamed += delegate
            {
                group.Name = paramNode.Header;
                anim.IsEdited = true;
            };
            paramNode.ContextMenus.Add(new MenuItem("Rename", () => paramNode.ActivateRename = true));

            paramNode.IsExpanded = true;

            List<BfresMaterialAnim.ParamTrack> tracks = new List<BfresMaterialAnim.ParamTrack>();
            if (group.Name.StartsWith("tex_mtx"))
            {
               tracks.Add(new BfresMaterialAnim.ParamTrack(4, 1, "Scale.X"));
               tracks.Add(new BfresMaterialAnim.ParamTrack(8, 1, "Scale.Y"));
               tracks.Add(new BfresMaterialAnim.ParamTrack(12, 0, "Rotate"));
               tracks.Add(new BfresMaterialAnim.ParamTrack(16, 0, "Position.X"));
               tracks.Add(new BfresMaterialAnim.ParamTrack(20, 0, "Position.Y"));
            }

            if (param != null)
            {
                switch (param.Type)
                {
                    case ShaderParamType.TexSrt:
                    case ShaderParamType.TexSrtEx:
                        var texSrt = ((TexSrt)param.DataValue);
                       tracks.Add(new BfresMaterialAnim.ParamTrack(0, (float)texSrt.Mode, "Mode"));
                       tracks.Add(new BfresMaterialAnim.ParamTrack(4, (float)texSrt.Scaling.X, "Scale.X"));
                       tracks.Add(new BfresMaterialAnim.ParamTrack(8, (float)texSrt.Scaling.Y, "Scale.Y"));
                       tracks.Add(new BfresMaterialAnim.ParamTrack(12, (float)texSrt.Rotation, "Rotate"));
                       tracks.Add(new BfresMaterialAnim.ParamTrack(16, (float)texSrt.Translation.X, "Position.X"));
                       tracks.Add(new BfresMaterialAnim.ParamTrack(20, (float)texSrt.Translation.Y, "Position.Y"));
                        break;
                    case ShaderParamType.Float:
                       tracks.Add(new BfresMaterialAnim.ParamTrack(0, (float)param.DataValue, "Value"));
                        break;
                    case ShaderParamType.Float2:
                    case ShaderParamType.Float3:
                    case ShaderParamType.Float4:
                        var values = ((float[])param.DataValue);
                        string[] channel = new string[4] { "X", "Y", "Z", "W" };
                        for (int i = 0; i < values.Length; i++)
                           tracks.Add(new BfresMaterialAnim.ParamTrack((uint)i * 4, values[i], channel[i]));
                        break;
                }
            }

            for (int i = 0; i  <tracks.Count; i++)
            {
                var targetTrack = group.Tracks.FirstOrDefault(x => ((BfresMaterialAnim.ParamTrack)x).ValueOffset ==tracks[i].ValueOffset);
                if (targetTrack == null)
                    group.Tracks.Add(tracks[i]);
                else
                    targetTrack.Name = tracks[i].Name;
            }

            foreach (BfresMaterialAnim.ParamTrack track in group.Tracks.OrderBy(x => ((BfresMaterialAnim.ParamTrack)x).ValueOffset))
            {
                track.ChannelIndex = ((int)track.ValueOffset / 4);

                var trackNode = new AnimationTree.TrackNode(anim, track);
                trackNode.Tag = track;
                trackNode.Icon = '\uf1b2'.ToString();
                paramNode.AddChild(trackNode);
            }

            return paramNode;
        }

        public class ColorTreeNode : AnimationTree.ColorGroupNode
        {
            public ColorTreeNode(STAnimation anim, STAnimGroup group, STAnimGroup parent) : base(anim, group, parent)
            {

            }

            public override void SetTrackColor(System.Numerics.Vector4 color)
            {
                InsertChannel(0, color.X);
                InsertChannel(4, color.Y);
                InsertChannel(8, color.Z);
            }

            private void InsertChannel(uint offset, float value)
            {
                var group = ((BfresMaterialAnim.ParamAnimGroup)Group);
                if (!group.Tracks.Any(x => ((BfresMaterialAnim.ParamTrack)x).ValueOffset == offset)) {
                    var track = new BfresMaterialAnim.ParamTrack()
                    {
                        ValueOffset = offset,
                    };
                    group.Tracks.Add(track);

                    //add to UI
                    var trackNode = new AnimationTree.TrackNode(Anim, track);
                    trackNode.Tag = track;
                    trackNode.Icon = '\uf1b2'.ToString();
                    this.AddChild(trackNode);
                }
                var target = group.Tracks.FirstOrDefault(x => ((BfresMaterialAnim.ParamTrack)x).ValueOffset == offset);
                var node = (AnimationTree.TrackNode)this.Children.FirstOrDefault(x => ((AnimationTree.TrackNode)x).Track == target);
                node.InsertOrUpdateKeyValue(value);
            }
        }

        public class SamplerTreeTrack : AnimationTree.TextureTrackNode
        {
            public override List<string> TextureList
            {
                get { return ((BfresMaterialAnim)Anim).TextureList; }
                set
                {
                    ((BfresMaterialAnim)Anim).TextureList = value;
                }
            }

            public SamplerTreeTrack(STAnimation anim, STAnimationTrack track, STAnimGroup parent) : base(anim, track)
            {
                Icon = '\uf03e'.ToString();
            }

            bool dialogOpened = false;

            public override void RenderNode()
            {
                ImGui.Text(this.Header);
                ImGui.NextColumn();

                var color = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                //Display keyed values differently
                bool isKeyed = Track.KeyFrames.Any(x => x.Frame == Anim.Frame);
                //   if (isKeyed)
                //   color = KEY_COLOR;

                ImGui.PushStyleColor(ImGuiCol.Text, color);

                //Span the whole column
                ImGui.PushItemWidth(ImGui.GetColumnWidth() - 14);

                string texture = GetTextureName(Anim.Frame);

                float size = ImGui.GetFrameHeight();
                if (ImGui.Button($"   {MapStudio.UI.IconManager.IMAGE_ICON}   "))
                {
                    dialogOpened = true;

                    var render = GLFrameworkEngine.DataCache.ModelCache.Values.FirstOrDefault();
                    TextureSelectionDialog.Textures = render.Textures;
                }
                ImGui.SameLine();

                var sourceTex = GetImage(texture);
                if (sourceTex != null) {
                   MapStudio.UI.IconManager.DrawTexture(sourceTex.Name, sourceTex);
                }
                ImGui.SameLine();
                if (ImGui.InputText("##texSelect", ref texture, 0x200))
                {

                }

                if (dialogOpened)
                {
                    if (TextureSelectionDialog.Render(texture, ref dialogOpened))
                    {
                        var input = TextureSelectionDialog.OutputName;
                        if (TextureList.IndexOf(input) == -1)
                            TextureList.Add(input);

                        InsertOrUpdateKeyValue(TextureList.IndexOf(input));
                    }
                }

                ImGui.PopItemWidth();

                ImGui.PopStyleColor();
                ImGui.NextColumn();
            }

            public override void RenderKeyTableUI()
            {
                ImGui.Columns(2);

                foreach (var key in this.Keys)
                {
                    DrawImageKey(key);
                }

                ImGui.NextColumn();

                var sourceTex = GetImage(GetTextureName(Anim.Frame));
                if (sourceTex != null)
                    DrawImageCanvas(sourceTex);

                ImGui.NextColumn();
                ImGui.Columns(1);
            }

            private void DrawImageKey(AnimationTree.KeyNode keyNode)
            {
                var pos = ImGui.GetCursorPos();

                string texture = GetTextureName(keyNode.Frame);
                var sourceTex = GetImage(texture);
                if (sourceTex != null) {
                    MapStudio.UI.IconManager.DrawTexture(sourceTex.Name, sourceTex, 32, 32);
                }
                ImGui.SameLine();

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{keyNode.Frame} / {this.Anim.FrameCount} {texture}");

                ImGui.SetCursorPos(pos);

                bool isSelected = keyNode.Frame == Anim.Frame;
                var size = new System.Numerics.Vector2(ImGui.GetColumnWidth(), 32);
                if (ImGui.Selectable($"##frame{keyNode.Frame}", isSelected, ImGuiSelectableFlags.None, size))
                {
                    Anim.Frame = keyNode.Frame;
                }

                    /*   if (ImGui.Button($"   {MapStudio.UI.IconManager.IMAGE_ICON}   "))
                       {
                           dialogOpened = true;

                           var render = GLFrameworkEngine.DataCache.ModelCache.Values.FirstOrDefault();
                           TextureSelectionDialog.Textures = render.Textures;
                       }
                       ImGui.SameLine();
                       */

                    if (dialogOpened)
                {
                    if (TextureSelectionDialog.Render(texture, ref dialogOpened))
                    {
                        var input = TextureSelectionDialog.OutputName;
                        if (TextureList.IndexOf(input) == -1)
                            TextureList.Add(input);

                        InsertOrUpdateKeyValue(TextureList.IndexOf(input));
                    }
                }
            }

            private void DrawImageCanvas(Toolbox.Core.STGenericTexture texture)
            {
                if (ImGui.BeginChild("##texture_dlg_canvas"))
                {
                    var size = ImGui.GetWindowSize();

                    //background
                    var pos = ImGui.GetCursorPos();
                    ImGui.Image((IntPtr)MapStudio.UI.IconManager.GetTextureIcon("CHECKERBOARD"), size);
                    //image

                    //Aspect size

                    #region Calculate Aspect Size
                    float tw, th, tx, ty;

                    int w = (int)texture.Width;
                    int h = (int)texture.Height;

                    double whRatio = (double)w / h;
                    if (texture.Width >= texture.Height)
                    {
                        tw = size.X;
                        th = (int)(tw / whRatio);
                    }
                    else
                    {
                        th = size.Y;
                        tw = (int)(th * whRatio);
                    }

                    //Rectangle placement
                    tx = (size.X - tw) / 2;
                    ty = (size.Y - th) / 2;

                    #endregion

                    ImGui.SetCursorPos(new System.Numerics.Vector2(pos.X, pos.Y + ty));
                    ImGui.Image((IntPtr)texture.RenderableTex.ID, new System.Numerics.Vector2(tw, th));
                }
                ImGui.EndChild();
            }

            public Toolbox.Core.STGenericTexture GetImage(string name)
            {
                foreach (var render in GLFrameworkEngine.DataCache.ModelCache.Values)
                {
                    if (render.Textures.ContainsKey(name))
                        return render.Textures[name].OriginalSource;
                }
                return null;
            }

            public string GetTextureName(float frame)
            {
                int index = (int)Track.GetFrameValue(frame);
                if (index >= TextureList.Count)
                    return "";
                return TextureList[index];
            }
        }
    }
}
