using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using Toolbox.Core;
using BfresLibrary;
using GLFrameworkEngine;
using CafeLibrary.Rendering;
using OpenTK;
using CafeLibrary.Rendering;
using Toolbox.Core.ViewModels;
using UIFramework;
using MapStudio.UI;

namespace CafeLibrary
{
    public class BfresVisibilityAnim : STAnimation, IEditableAnimation, IContextMenu
    {
        //Root for animation tree
        public TreeNode Root { get; set; }

        private string ModelName = null;

        public int KeyIndex = 0;

        public NodeBase UINode;

        public ResFile ResFile { get; set; }
        public VisibilityAnim VisibilityAnim;

        private int Hash;

        public BfresVisibilityAnim(ResFile resFile, VisibilityAnim anim, string name)
        {
            ResFile = resFile;
            VisibilityAnim = anim;
            ModelName = name;
            CanPlay = false; //Default all animations to not play unless toggled in list
            UINode = new NodeBase();
            Root = new TreeNode();
            UINode.CanRename = true;
            UINode.Icon = '\uf06e'.ToString();
            UINode.IconColor = new System.Numerics.Vector4(0.3f, 0.75f, 0.9f, 1);
            UINode.OnHeaderRenamed += delegate
            {
                OnRenamed(UINode.Header);
            };
            Reload(anim);
        }

        public void OnSave()
        {
            VisibilityAnim.FrameCount = (int)this.FrameCount;

            var hash = BoneVisAnimConverter.CalculateHash(this);
            Console.WriteLine($"Bone vis hash {hash} current hash {Hash}");
            if (hash != Hash)
            {
                BoneVisAnimConverter.ConvertAnimation(this, VisibilityAnim);
                //update with new hash.
                Hash = hash;
            }
        }

        public void OnRenamed(string name)
        {
            string previousName = VisibilityAnim.Name;

            Root.Header = name;
            VisibilityAnim.Name = name;
            if (ResFile.BoneVisibilityAnims.ContainsKey(previousName))
            {
                ResFile.BoneVisibilityAnims.RemoveKey(previousName);
                ResFile.BoneVisibilityAnims.Add(VisibilityAnim.Name, VisibilityAnim);
            }
        }

        public MenuItemModel[] GetContextMenuItems()
        {
            return new MenuItemModel[]
            {
                new MenuItemModel("Export", ExportAction),
                new MenuItemModel("Replace", ReplaceAction),
                new MenuItemModel(""),
                new MenuItemModel("Rename", () => UINode.ActivateRename = true),
                new MenuItemModel(""),
                new MenuItemModel("Delete", DeleteAction)
            };
        }

        private void ExportAction()
        {
            var dlg = new ImguiFileDialog();
            dlg.SaveDialog = true;
            dlg.FileName = $"{VisibilityAnim.Name}.json";
            dlg.AddFilter(".bfbvi", ".bfbvi");
            dlg.AddFilter(".json", ".json");

            if (dlg.ShowDialog())
            {
                 OnSave();
                VisibilityAnim.Export(dlg.FilePath, ResFile);
            }
        }

        private void ReplaceAction()
        {
            var dlg = new ImguiFileDialog();
            dlg.SaveDialog = false;
            dlg.FileName = $"{VisibilityAnim.Name}.json";
            dlg.AddFilter(".bfbvi", ".bfbvi");
            dlg.AddFilter(".json", ".json");

            if (dlg.ShowDialog())
            {
                VisibilityAnim.Import(dlg.FilePath, ResFile);
                VisibilityAnim.Name = this.Name;

                Reload(VisibilityAnim);
            }
        }

        private void DeleteAction()
        {
            int result = TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to remove these animations? Operation cannot be undone.");
            if (result != 1)
                return;

            UINode.Parent.Children.Remove(UINode);

            if (ResFile.BoneVisibilityAnims.ContainsValue(this.VisibilityAnim))
                ResFile.BoneVisibilityAnims.Remove(this.VisibilityAnim);
        }

        public override void NextFrame()
        {
            foreach (BoneAnimGroup group in AnimGroups)
            {
                var skeletons = GetActiveSkeletons();
                foreach (var skeleton in skeletons)
                {
                    var bone = skeleton.SearchBone(group.Name);
                    if (bone == null)
                        continue;

                    ParseKeyTrack(bone, group);
                }
            }
        }

        public override void Reset()
        {
            var skeletons = GetActiveSkeletons();
            foreach (var skeleton in skeletons)
            {
                foreach (var bone in skeleton.Bones)
                    bone.Visible = true;
            }
        }

        private void ParseKeyTrack(STBone bone, BoneAnimGroup group)
        {
            float value = group.Track.GetFrameValue(this.Frame);
            bool isVisible = value != 0;
            bone.Visible = isVisible;
        }

        public void Reload(VisibilityAnim anim)
        {
            Name = anim.Name;
            FrameCount = anim.FrameCount;
            FrameRate = 60.0f;
            Loop = anim.Flags.HasFlag(VisibilityAnimFlags.Looping);
            UINode.Header = anim.Name;
            UINode.Tag = this;

            if (anim.Names == null)
                return;

            AnimGroups.Clear();
            for (int i = 0; i < anim.Names.Count; i++)
            {
                var baseValue = anim.BaseDataList[i];

                BoneAnimGroup group = new BoneAnimGroup();
                group.Name = anim.Names[i];
                AnimGroups.Add(group);

                group.Track.Name = group.Name;
                group.Track.KeyFrames.Add(new STKeyFrame() { Frame = 0, Value = baseValue ? 1 : 0 });

                foreach (var curve in anim.Curves)
                {
                    if (curve.AnimDataOffset == i)
                        BfresAnimations.GenerateKeys(group.Track, curve);
                }
            }

            Root.Children.Clear();

            if (ResFile != null)
                BoneVisAnimUI.ReloadTree(Root, this, VisibilityAnim);

            Hash = BoneVisAnimConverter.CalculateHash(this);
        }

        public class BoneAnimGroup : STAnimGroup
        {
            public BfresAnimationTrack Track = new BfresAnimationTrack();
        }

        private List<BfresMeshRender> GetMeshes(string name)
        {
            if (!DataCache.ModelCache.ContainsKey(ModelName))
                return new List<BfresMeshRender>();

            List<BfresMeshRender> meshes = new List<BfresMeshRender>();

            var bfres = (BfresRender)DataCache.ModelCache[ModelName];

            //Don't animate objects out of the screen fustrum
            //  if (!bfres.InFustrum) return materials;

            foreach (BfresModelRender model in bfres.Models)
            {
                foreach (BfresMeshRender mesh in model.Meshes)
                {
                    if (mesh.Name == name)
                        meshes.Add(mesh);
                }
            }

            return meshes;
        }

        public STSkeleton[] GetActiveSkeletons()
        {
            List<STSkeleton> skeletons = new List<STSkeleton>();
            foreach (BfresRender render in DataCache.ModelCache.Values)
            {
                foreach (BfresModelRender model in render.Models)
                    skeletons.Add(model.ModelData.Skeleton);
            }
            return skeletons.ToArray();
        }
    }
}