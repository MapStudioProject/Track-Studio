using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary;
using Toolbox.Core.ViewModels;
using CafeLibrary.Rendering;
using Toolbox.Core;
using MapStudio.UI;
using Toolbox.Core.Animations;

namespace CafeLibrary
{
    public class SceneAnimFolder : SubSectionBase
    {
        public override string Header => "Scene Animations";

        public SceneAnimFolder() { }

        ResFile ResFile;

        public void Load(ResFile resFile)
        {
            ResFile = resFile;
            foreach (SceneAnim anim in resFile.SceneAnims.Values)
            {
                if (this.Children.Any(x => x.Tag == anim))
                    continue;

                var node = new SceneAnimNode(resFile, anim);
                AddChild(node);
            }
        }

        public override void Replace(NodeBase node, string filePath)
        {
            
        }

        public override void Clear()
        {
            ResFile.SceneAnims.Clear();
            this.Children.Clear();
        }

        public class SceneAnimNode : NodeBase
        {
            public SceneAnim SceneAnim;
            public ResFile ResFile;

            public SceneAnimNode(ResFile resFile, SceneAnim sceneAnim)
            {
                this.ResFile = resFile;
                this.SceneAnim = sceneAnim;
                this.Header = SceneAnim.Name;

                this.Icon = '\uf187'.ToString();
                this.Tag = sceneAnim;
                this.OnHeaderRenamed += delegate
                {
                    OnRenamed(this.Header);
                };
                this.ContextMenus.AddRange(GetContextMenuItems());
                this.CanRename = true;
                this.Reload();
            }

            public void OnSave()
            {
                foreach (var animNode in this.Children)
                {
                    if (animNode.Tag is BfresCameraAnim)
                        ((BfresCameraAnim)animNode.Tag).OnSave();
                }
            }

            private void OnRenamed(string name)
            {
                //not changed
                if (SceneAnim.Name == name)
                    return;

                //Dupe name
                if (ResFile.SceneAnims.ContainsKey(this.Header))
                {
                    TinyFileDialog.MessageBoxErrorOk($"Name {this.Header} already exists!");
                    //revert
                    this.Header = SceneAnim.Name;
                    return;
                }

                string previousName = SceneAnim.Name;
                SceneAnim.Name = name;
                //Adjust dictionary
                if (ResFile.SceneAnims.ContainsKey(previousName))
                {
                    ResFile.SceneAnims.RemoveKey(previousName);
                    ResFile.SceneAnims.Add(SceneAnim.Name, SceneAnim);
                }
            }

            public MenuItemModel[] GetContextMenuItems()
            {
                return new MenuItemModel[]
                {
                new MenuItemModel("Export", ExportAction),
                new MenuItemModel("Replace", ReplaceAction),
                new MenuItemModel(""),
                new MenuItemModel("Rename", () => this.ActivateRename = true),
                new MenuItemModel(""),
                new MenuItemModel("Add Camera Anim (Lookat)",() => AddCameraAnim(false)),
                new MenuItemModel("Add Camera Anim (Euler)",() => AddCameraAnim(true)),
                new MenuItemModel(""),
                new MenuItemModel("Delete", DeleteAction)
                };
            }

            private void ExportAction()
            {
                var dlg = new ImguiFileDialog();
                dlg.SaveDialog = true;
                dlg.FileName = $"{SceneAnim.Name}.json";
                dlg.AddFilter(".bfscn", ".bfscn");
                dlg.AddFilter(".json", ".json");

                if (dlg.ShowDialog())
                {
                    SceneAnim.Export(dlg.FilePath, ResFile);
                }
            }

            private void ReplaceAction()
            {
                var dlg = new ImguiFileDialog();
                dlg.SaveDialog = false;
                dlg.FileName = $"{SceneAnim.Name}.json";
                dlg.AddFilter(".bfscn", ".bfscn");
                dlg.AddFilter(".json", ".json");

                if (dlg.ShowDialog())
                {
                    SceneAnim.Import(dlg.FilePath, ResFile);
                    SceneAnim.Name = this.Header;
                    Reload();
                }
            }

            private void AddCameraAnim(bool isEuler)
            {
                var anim = new CameraAnim() { Name = "CameraAnim",
                    Flags = CameraAnimFlags.Perspective,
                    FrameCount = 100,
                    BaseData = new CameraAnimData()
                {
                        ClipNear = 1.0f,
                        ClipFar = 100000f,
                        FieldOfView = 0.6981317f,
                        Position = new Syroot.Maths.Vector3F(),
                        Rotation = new Syroot.Maths.Vector3F(),
                        AspectRatio = 1.4999471f, //16:9
                        Twist = 0.0f,
                }};

                if (isEuler)
                    anim.Flags = CameraAnimFlags.EulerZXY;

                anim.Name = Utils.RenameDuplicateString(anim.Name, SceneAnim.CameraAnims.Keys.Select(x => x).ToList());
                SceneAnim.CameraAnims.Add(anim.Name, anim);

                var an = new BfresCameraAnim(ResFile, SceneAnim, anim);
                this.AddChild(an.UINode);
            }

            private void DeleteAction()
            {
                int result = TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to remove these animations? Operation cannot be undone.");
                if (result != 1)
                    return;

                this.Parent.Children.Remove(this);

                if (this.ResFile.SceneAnims.ContainsValue(this.SceneAnim))
                    this.ResFile.SceneAnims.Remove(this.SceneAnim);
            }

            private void Reload()
            {
                this.Children.Clear();
                foreach (CameraAnim camAnim in SceneAnim.CameraAnims.Values)
                {
                    var an = new BfresCameraAnim(ResFile, SceneAnim, camAnim);
                    this.AddChild(an.UINode);
                }
                foreach (LightAnim lightAnim in SceneAnim.LightAnims.Values)
                {
                    var camnode = new NodeBase(lightAnim.Name);
                    this.AddChild(camnode);
                }
                foreach (FogAnim fogAnim in SceneAnim.FogAnims.Values)
                {
                    var camnode = new NodeBase(fogAnim.Name);
                    this.AddChild(camnode);
                }
            }
        }
    }
}