using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary;
using Toolbox.Core.ViewModels;
using CafeLibrary.Rendering;

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

                var node = new NodeBase(anim.Name);
                node.Icon = '\uf187'.ToString();
                node.Tag = anim;
                AddChild(node);

                foreach (CameraAnim camAnim in anim.CameraAnims.Values)
                {
                    var an = new BfresCameraAnim(resFile, anim, camAnim);
                    node.AddChild(an.UINode);
                }
                foreach (LightAnim lightAnim in anim.LightAnims.Values)
                {
                    var camnode = new NodeBase(lightAnim.Name);
                    node.AddChild(camnode);
                }
                foreach (FogAnim fogAnim in anim.FogAnims.Values)
                {
                    var camnode = new NodeBase(fogAnim.Name);
                    node.AddChild(camnode);
                }
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
    }
}