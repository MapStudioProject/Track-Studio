using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using Toolbox.Core;
using BfresLibrary;
using GLFrameworkEngine;
using OpenTK;
using MapStudio.UI;
using Toolbox.Core.ViewModels;
using UIFramework;

namespace CafeLibrary.Rendering
{
    public class BfresCameraAnim : CameraAnimation, IEditableAnimation, IContextMenu
    {
        public NodeBase UINode;

        //Root for animation tree
        public TreeNode Root { get; set; } = new TreeNode();

        public ResFile ResFile;
        public CameraAnim CameraAnim;
        public SceneAnim SceneAnim;

        private int Hash;

        public BfresCameraAnim(ResFile resFile, SceneAnim sceneAnim, CameraAnim anim)
        {
            SceneAnim = sceneAnim;
            CameraAnim = anim;
            ResFile = resFile;

            UINode = new NodeBase(anim.Name);
            UINode.Tag = this;
            UINode.Icon = '\uf03d'.ToString();
            UINode.CanRename = true;
            UINode.OnHeaderRenamed += delegate
            {
                OnRenamed(UINode.Header);
            };

            CanPlay = false; //Default all animations to not play unless toggled in list
            Reload(anim);

            Root = CameraAnimUI.ReloadTree(Root, this, sceneAnim);
            Hash = CameraAnimConverter.CalculateHash(this);
        }

        public BfresCameraAnim(CameraAnim anim)
        {
            UINode = new NodeBase(anim.Name);
            UINode.Tag = this;

            CanPlay = false; //Default all animations to not play unless toggled in list
            Reload(anim);
        }

        public void OnRenamed(string name)
        {
            //not changed
            if (CameraAnim.Name == name)
                return;

            //Dupe name
            if (SceneAnim.CameraAnims.ContainsKey(UINode.Header))
            {
                TinyFileDialog.MessageBoxErrorOk($"Name {UINode.Header} already exists!");
                //revert
                UINode.Header = CameraAnim.Name;
                return;
            }

            string previousName = CameraAnim.Name;

            Root.Header = name;
            CameraAnim.Name = name;

            //Update dictionary
            if (SceneAnim.CameraAnims.ContainsKey(previousName))
            {
                SceneAnim.CameraAnims.RemoveKey(previousName);
                SceneAnim.CameraAnims.Add(CameraAnim.Name, CameraAnim);
            }
        }

        public void OnSave()
        {
            CameraAnim.FrameCount = (int)this.FrameCount;

            var hash = CameraAnimConverter.CalculateHash(this);
            Console.WriteLine($"Bone vis hash {hash} current hash {Hash}");
            if (hash != Hash)
            {
                CameraAnimConverter.ConvertAnimation(this, CameraAnim);
                //update with new hash.
                Hash = hash;
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
            dlg.FileName = $"{CameraAnim.Name}.bcam";
            dlg.AddFilter(".bcam", ".bcam");
        //    dlg.AddFilter(".json", ".json");

            if (dlg.ShowDialog())
            {
                OnSave();
                CameraAnim.Export(dlg.FilePath, ResFile);
            }
        }

        private void ReplaceAction()
        {
            var dlg = new ImguiFileDialog();
            dlg.SaveDialog = false;
            dlg.FileName = $"{CameraAnim.Name}.json";
            dlg.AddFilter(".bcam", ".bcam");
         //   dlg.AddFilter(".json", ".json");

            if (dlg.ShowDialog())
            {
                CameraAnim.Import(dlg.FilePath, ResFile);
                CameraAnim.Name = this.UINode.Header;

                //Update types if needed as can be broken by user error
                foreach (var curve in CameraAnim.Curves)
                {
                    var maxFrame = curve.Frames.Max(x => x);
                    if (maxFrame > 255 && curve.FrameType == AnimCurveFrameType.Byte)
                        curve.FrameType = AnimCurveFrameType.Decimal10x5;
                    if (maxFrame > ushort.MaxValue)
                        curve.FrameType = AnimCurveFrameType.Single;
                }

                Reload(CameraAnim);
            }
        }

        private void DeleteAction()
        {
            int result = TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to remove these animations? Operation cannot be undone.");
            if (result != 1)
                return;

            UINode.Parent.Children.Remove(UINode);

            if (this.SceneAnim.CameraAnims.ContainsValue(this.CameraAnim))
                this.SceneAnim.CameraAnims.Remove(this.CameraAnim);
        }

        public override void NextFrame(GLContext context)
        {
            return;

            var group = AnimGroups[0] as CameraAnimGroup;
            var camera = context.Camera;

            var posX = group.PositionX.GetFrameValue(this.Frame) * GLContext.PreviewScale;
            var posY = group.PositionY.GetFrameValue(this.Frame) * GLContext.PreviewScale;
            var posZ = group.PositionZ.GetFrameValue(this.Frame) * GLContext.PreviewScale;
            var rotX = group.RotationX.GetFrameValue(this.Frame);
            var rotY = group.RotationY.GetFrameValue(this.Frame);
            var rotZ = group.RotationZ.GetFrameValue(this.Frame);
            var twist = group.Twist.GetFrameValue(this.Frame);
            var near = group.ClipNear.GetFrameValue(this.Frame);
            var far = group.ClipFar.GetFrameValue(this.Frame);
            var fov = group.FieldOfView.GetFrameValue(this.Frame);
            var aspect = group.AspectRatio.GetFrameValue(this.Frame);

            camera.SetKeyframe(CameraAnimationKeys.PositionX, posX);
            camera.SetKeyframe(CameraAnimationKeys.PositionY, posY);
            camera.SetKeyframe(CameraAnimationKeys.PositionZ, posZ);
            camera.SetKeyframe(CameraAnimationKeys.Near, near);
            camera.SetKeyframe(CameraAnimationKeys.Far, far);
            camera.SetKeyframe(CameraAnimationKeys.FieldOfView, fov);
            camera.SetKeyframe(CameraAnimationKeys.Distance, 0);
            camera.RotationLookat = group.IsLookat;

            if (group.IsLookat)
            {
                camera.SetKeyframe(CameraAnimationKeys.RotationX, 0);
                camera.SetKeyframe(CameraAnimationKeys.RotationY, 0);
                camera.SetKeyframe(CameraAnimationKeys.RotationZ, 0);
                camera.SetKeyframe(CameraAnimationKeys.Twist, twist);
                //XYZ values for eye target placement
                camera.SetKeyframe(CameraAnimationKeys.TargetX, rotX * GLContext.PreviewScale);
                camera.SetKeyframe(CameraAnimationKeys.TargetY, rotY * GLContext.PreviewScale);
                camera.SetKeyframe(CameraAnimationKeys.TargetZ, rotZ * GLContext.PreviewScale);
            }
            else
            {
                camera.SetKeyframe(CameraAnimationKeys.RotationX, rotX);
                camera.SetKeyframe(CameraAnimationKeys.RotationY, rotY);
                camera.SetKeyframe(CameraAnimationKeys.RotationZ, rotZ);
                camera.SetKeyframe(CameraAnimationKeys.Twist, 0);
                camera.SetKeyframe(CameraAnimationKeys.TargetX, 0);
                camera.SetKeyframe(CameraAnimationKeys.TargetY, 0);
                camera.SetKeyframe(CameraAnimationKeys.TargetZ, 0);
            }
            GLContext.ActiveContext.UpdateViewport = true;

            camera.UpdateMatrices();
        }

        public void Reload(CameraAnim anim)
        {
            Name = anim.Name;
            FrameCount = anim.FrameCount;
            FrameRate = 60.0f;
            Loop = anim.Flags.HasFlag(CameraAnimFlags.Looping);

            CameraAnimGroup group = new CameraAnimGroup();
            group.Name = anim.Name;
            group.IsOrtho = !anim.Flags.HasFlag(CameraAnimFlags.Perspective);
            group.IsLookat = !anim.Flags.HasFlag(CameraAnimFlags.EulerZXY);

            group.PositionX.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Position.X));
            group.PositionY.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Position.Y));
            group.PositionZ.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Position.Z));
            group.RotationX.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Rotation.X));
            group.RotationY.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Rotation.Y));
            group.RotationZ.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Rotation.Z));
            group.Twist.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Twist));
            group.ClipNear.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.ClipNear));
            group.ClipFar.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.ClipFar));
            group.AspectRatio.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.AspectRatio));
            group.FieldOfView.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.FieldOfView));

            AnimGroups.Clear();
            AnimGroups.Add(group);
            for (int i = 0; i < anim.Curves.Count; i++)
            {
                var curve = anim.Curves[i];
                switch ((CameraAnimDataOffset)curve.AnimDataOffset)
                {
                    case CameraAnimDataOffset.PositionX: BfresAnimations.GenerateKeys(group.PositionX, curve); break;
                    case CameraAnimDataOffset.PositionY: BfresAnimations.GenerateKeys(group.PositionY, curve); break;
                    case CameraAnimDataOffset.PositionZ: BfresAnimations.GenerateKeys(group.PositionZ, curve); break;
                    case CameraAnimDataOffset.RotationX: BfresAnimations.GenerateKeys(group.RotationX, curve); break;
                    case CameraAnimDataOffset.RotationY: BfresAnimations.GenerateKeys(group.RotationY, curve); break;
                    case CameraAnimDataOffset.RotationZ: BfresAnimations.GenerateKeys(group.RotationZ, curve); break;
                    case CameraAnimDataOffset.Twist: BfresAnimations.GenerateKeys(group.Twist, curve); break;
                    case CameraAnimDataOffset.ClipNear: BfresAnimations.GenerateKeys(group.ClipNear, curve); break;
                    case CameraAnimDataOffset.ClipFar: BfresAnimations.GenerateKeys(group.ClipFar, curve); break;
                    case CameraAnimDataOffset.AspectRatio: BfresAnimations.GenerateKeys(group.AspectRatio, curve); break;
                    case CameraAnimDataOffset.FieldOFView: BfresAnimations.GenerateKeys(group.FieldOfView, curve); break;
                }
            }
        }

        public class CameraAnimGroup : STAnimGroup
        {
            public BfresAnimationTrack ClipNear = new BfresAnimationTrack();
            public BfresAnimationTrack ClipFar = new BfresAnimationTrack();
            public BfresAnimationTrack AspectRatio = new BfresAnimationTrack();
            public BfresAnimationTrack FieldOfView = new BfresAnimationTrack();
            public BfresAnimationTrack PositionX = new BfresAnimationTrack();
            public BfresAnimationTrack PositionY = new BfresAnimationTrack();
            public BfresAnimationTrack PositionZ = new BfresAnimationTrack();

            public BfresAnimationTrack RotationX = new BfresAnimationTrack();
            public BfresAnimationTrack RotationY = new BfresAnimationTrack();
            public BfresAnimationTrack RotationZ = new BfresAnimationTrack();

            public BfresAnimationTrack Twist = new BfresAnimationTrack();

            public bool IsLookat = false;
            public bool IsOrtho = false;

            public override List<STAnimationTrack> GetTracks()
            {
                List<STAnimationTrack> tracks = new List<STAnimationTrack>();
                tracks.Add(this.PositionX);
                tracks.Add(this.PositionY);
                tracks.Add(this.PositionZ);
                tracks.Add(this.RotationX);
                tracks.Add(this.RotationY);
                tracks.Add(this.RotationZ);
                tracks.Add(this.ClipFar);
                tracks.Add(this.ClipNear);
                tracks.Add(this.AspectRatio);
                tracks.Add(this.FieldOfView);
                tracks.Add(this.Twist);
                return tracks;
            }
        }
    }
}
