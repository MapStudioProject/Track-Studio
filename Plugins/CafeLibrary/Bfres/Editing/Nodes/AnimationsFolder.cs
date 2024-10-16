using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.ViewModels;
using Toolbox.Core.Animations;
using Toolbox.Core;
using BfresLibrary;
using CafeLibrary.Rendering;
using static CafeLibrary.SceneAnimFolder;

namespace CafeLibrary
{
    public class AnimationsFolder : NodeBase
    {
        public override string Header => "Animations";

        private readonly NodeBase SkeletalAnimsFolder = new NodeBase("Skeletal Animations");
        private readonly NodeBase ShaderParamAnimsFolder = new NodeBase("Shader Param Animations");
        private readonly NodeBase ColorParamAnimsFolder = new NodeBase("Color Param Animations");
        private readonly NodeBase TexSRTParamAnimsFolder = new NodeBase("Texture SRT Animations");
        private readonly NodeBase TexPatternAnimsFolder = new NodeBase("Texture Pattern Animations");
        private readonly NodeBase BoneVisAnimsFolder = new NodeBase("Bone Visibility Animations");
        private readonly SceneAnimFolder SceneAnimsFolder = new SceneAnimFolder();

        private BFRES BfresWrapper;
        private ResFile ResFile;

        public AnimationsFolder(BFRES bfres, ResFile resFile)
        {
            BfresWrapper = bfres;
            ResFile = resFile;
            ContextMenus.Add(new MenuItemModel("New Skeleton Animation", AddSkeletalAnim));
            ContextMenus.Add(new MenuItemModel("New Texture Pattern Animation", AddTextureAnim));
            ContextMenus.Add(new MenuItemModel("New Shader Param Animation", AddShaderParamAnim));
            ContextMenus.Add(new MenuItemModel("New Tex SRT Animation", AddTexSRTAnim));
            ContextMenus.Add(new MenuItemModel("New Color Animation", AddColorAnim));
            ContextMenus.Add(new MenuItemModel("New Bone Vis Animation", AddVisAnimation));
            ContextMenus.Add(new MenuItemModel("New Scene Animation", AddSceneAnimation));

            Reload();
        }

        public void Reload(MaterialAnim anim)
        {
            foreach (var animFolder in this.Children)
            {
                foreach (var child in animFolder.Children)
                {
                    if (!(child.Tag is BfresMaterialAnim))
                        continue;

                    if (((BfresMaterialAnim)child.Tag).Name == anim.Name)
                        ((BfresMaterialAnim)child.Tag).Reload(anim);
                }
            }
        }

        private void AddSkeletalAnim()
        {
            var anim = new SkeletalAnim() { Name = "SkeletalAnim_auto", FlagsRotate = SkeletalAnimFlagsRotate.EulerXYZ };
            anim.Name = Utils.RenameDuplicateString(anim.Name, ResFile.SkeletalAnims.Keys.Select(x => x).ToList());
            ResFile.SkeletalAnims.Add(anim.Name, anim);

            AddSkeletalAnimation(anim);

            if (SkeletalAnimsFolder.Parent == null) AddChild(SkeletalAnimsFolder);
        }

        private void AddTextureAnim()
        {
            var anim = new MaterialAnim() { Name = "ef_TextureAnim_auto" };
            anim.Name = Utils.RenameDuplicateString(anim.Name, ResFile.TexPatternAnims.Keys.Select(x => x).ToList());
            ResFile.TexPatternAnims.Add(anim.Name, anim);

            AddTexturePatternAnimation(anim);

            if (TexPatternAnimsFolder.Parent == null) AddChild(TexPatternAnimsFolder);
        }

        private void AddColorAnim()
        {
            var anim = new MaterialAnim() { Name = "ef_ColorAnim_auto" };
            anim.Name = Utils.RenameDuplicateString(anim.Name, ResFile.ColorAnims.Keys.Select(x => x).ToList());
            ResFile.ColorAnims.Add(anim.Name, anim);

            AddColorAnimation(anim);

            if (ColorParamAnimsFolder.Parent == null) AddChild(ColorParamAnimsFolder);
        }

        private void AddTexSRTAnim()
        {
            var anim = new MaterialAnim() { Name = "ef_TexSRTAnim_auto" };
            anim.Name = Utils.RenameDuplicateString(anim.Name, ResFile.TexSrtAnims.Keys.Select(x => x).ToList());
            ResFile.TexSrtAnims.Add(anim.Name, anim);

            AddTextureSRTAnimation(anim);

            if (TexSRTParamAnimsFolder.Parent == null) AddChild(TexSRTParamAnimsFolder);
        }

        private void AddShaderParamAnim()
        {
            var anim = new MaterialAnim() { Name = "ef_ShaderParamAnim_auto" };
            anim.Name = Utils.RenameDuplicateString(anim.Name, ResFile.ShaderParamAnims.Keys.Select(x => x).ToList());
            ResFile.ShaderParamAnims.Add(anim.Name, anim);

            AddShaderParamAnimation(anim);

            if (ShaderParamAnimsFolder.Parent == null) AddChild(ShaderParamAnimsFolder);
        }

        private void AddVisAnimation()
        {
            var anim = new VisibilityAnim() { Name = "VisibilityAnim" };
            anim.Name = Utils.RenameDuplicateString(anim.Name, ResFile.BoneVisibilityAnims.Keys.Select(x => x).ToList());
            ResFile.BoneVisibilityAnims.Add(anim.Name, anim);

            AddVisAnimation(anim);

            if (BoneVisAnimsFolder.Parent == null) AddChild(BoneVisAnimsFolder);
        }

        private void AddSceneAnimation()
        {
            var anim = new SceneAnim() { Name = "SceneAnim" };
            anim.Name = Utils.RenameDuplicateString(anim.Name, ResFile.SceneAnims.Keys.Select(x => x).ToList());
            ResFile.SceneAnims.Add(anim.Name, anim);

            AddSceneAnimation(anim);

            if (SceneAnimsFolder.Parent == null) AddChild(SceneAnimsFolder);
        }

        public void OnSave()
        {
            foreach (var anim in SkeletalAnimsFolder.Children)
                ((BfresSkeletalAnim)anim.Tag).OnSave();
            foreach (var anim in ShaderParamAnimsFolder.Children)
                ((BfresMaterialAnim)anim.Tag).OnSave();
            foreach (var anim in ColorParamAnimsFolder.Children)
                ((BfresMaterialAnim)anim.Tag).OnSave();
            foreach (var anim in TexSRTParamAnimsFolder.Children)
                ((BfresMaterialAnim)anim.Tag).OnSave();
            foreach (var anim in TexPatternAnimsFolder.Children)
                ((BfresMaterialAnim)anim.Tag).OnSave();
            foreach (var anim in BoneVisAnimsFolder.Children)
                ((BfresVisibilityAnim)anim.Tag).OnSave();
            foreach (var anim in SceneAnimsFolder.Children)
                ((SceneAnimNode)anim).OnSave();
        }

        public void Reload()
        {
            Children.Clear();

            foreach (var anim in ResFile.SkeletalAnims.Values)
                if (!SkeletalAnimsFolder.Children.Any(x => x.Tag == anim))
                    AddSkeletalAnimation(anim);
            foreach (var anim in ResFile.ShaderParamAnims.Values)
                if (!ShaderParamAnimsFolder.Children.Any(x => ((BfresMaterialAnim)x.Tag).MaterialAnim == anim))
                    AddShaderParamAnimation(anim);
            foreach (var anim in ResFile.ColorAnims.Values)
                if (!ColorParamAnimsFolder.Children.Any(x => ((BfresMaterialAnim)x.Tag).MaterialAnim == anim))
                    AddColorAnimation(anim);
            foreach (var anim in ResFile.TexSrtAnims.Values)
                if (!TexSRTParamAnimsFolder.Children.Any(x => ((BfresMaterialAnim)x.Tag).MaterialAnim == anim))
                    AddTextureSRTAnimation(anim);
            foreach (var anim in ResFile.TexPatternAnims.Values)
                if (!TexPatternAnimsFolder.Children.Any(x => ((BfresMaterialAnim)x.Tag).MaterialAnim == anim))
                    AddTexturePatternAnimation(anim);
            foreach (var anim in ResFile.BoneVisibilityAnims.Values)
                if (!BoneVisAnimsFolder.Children.Any(x => x.Tag == anim))
                    AddVisAnimation(anim);

            SceneAnimsFolder.Load(ResFile);

            if (SkeletalAnimsFolder.Children.Count > 0) AddChild(SkeletalAnimsFolder);
            if (ShaderParamAnimsFolder.Children.Count > 0) AddChild(ShaderParamAnimsFolder);
            if (ColorParamAnimsFolder.Children.Count > 0) AddChild(ColorParamAnimsFolder);
            if (TexSRTParamAnimsFolder.Children.Count > 0) AddChild(TexSRTParamAnimsFolder);
            if (TexPatternAnimsFolder.Children.Count > 0) AddChild(TexPatternAnimsFolder);
            if (SceneAnimsFolder.Children.Count > 0) AddChild(SceneAnimsFolder);
            if (BoneVisAnimsFolder.Children.Count > 0) AddChild(BoneVisAnimsFolder);
        }

        private void AddColorAnimation(MaterialAnim anim)
        {
            var fmaa = new BfresMaterialAnim(ResFile, ResFile.ColorAnims, anim, BfresWrapper.Renderer.Name);
            BfresWrapper.Renderer.MaterialAnimations.Add(fmaa);
            ColorParamAnimsFolder.AddChild(fmaa.UINode);
        }

        private void AddShaderParamAnimation(MaterialAnim anim)
        {
            var fmaa = new BfresMaterialAnim(ResFile, ResFile.ShaderParamAnims, anim, BfresWrapper.Renderer.Name);
            BfresWrapper.Renderer.MaterialAnimations.Add(fmaa);
            ShaderParamAnimsFolder.AddChild(fmaa.UINode);
        }

        private void AddTextureSRTAnimation(MaterialAnim anim)
        {
            var fmaa = new BfresMaterialAnim(ResFile, ResFile.TexSrtAnims, anim, BfresWrapper.Renderer.Name);
            BfresWrapper.Renderer.MaterialAnimations.Add(fmaa);
            TexSRTParamAnimsFolder.AddChild(fmaa.UINode);
        }

        private void AddTexturePatternAnimation(MaterialAnim anim)
        {
            var fmaa = new BfresMaterialAnim(ResFile, ResFile.TexPatternAnims, anim, BfresWrapper.Renderer.Name);
            BfresWrapper.Renderer.MaterialAnimations.Add(fmaa);
            TexPatternAnimsFolder.AddChild(fmaa.UINode);
        }

        private void AddSkeletalAnimation(SkeletalAnim anim)
        {
            var fska = new BfresSkeletalAnim(ResFile, anim, BfresWrapper.Renderer.Name);
            BfresWrapper.Renderer.SkeletalAnimations.Add(fska);
            SkeletalAnimsFolder.AddChild(fska.UINode);
        }

        private void AddVisAnimation(VisibilityAnim anim)
        {
            var bvba = new BfresVisibilityAnim(ResFile, anim, BfresWrapper.Renderer.Name);
            BfresWrapper.Renderer.VisAnimations.Add(bvba);
            BoneVisAnimsFolder.AddChild(bvba.UINode);
        }

        private void AddSceneAnimation(SceneAnim anim)
        {
            SceneAnimNode sceneAnimNode = new SceneAnimNode(ResFile, anim);
            sceneAnimNode.Tag = anim;

            foreach (var camAnim in anim.CameraAnims.Values)
            {
                var an = new BfresCameraAnim(camAnim);
                BfresWrapper.Renderer.CameraAnimations.Add(an);

                sceneAnimNode.AddChild(an.UINode);
            }

            SceneAnimsFolder.AddChild(sceneAnimNode);
        }
    }
}
