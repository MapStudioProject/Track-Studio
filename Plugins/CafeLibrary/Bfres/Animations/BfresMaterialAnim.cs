using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using Toolbox.Core;
using BfresLibrary;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;
using MapStudio.UI;
using UIFramework;

namespace CafeLibrary.Rendering
{
    public class BfresMaterialAnim : STAnimation, IContextMenu, IPropertyUI, IEditableAnimation
    {
        private string ModelName = null;

        private int Hash;

        //Root for animation tree
        public TreeNode Root { get; set; }

        public List<string> TextureList = new List<string>();

        public MaterialAnim MaterialAnim;

        public ResFile ResFile { get; set; }

        public NodeBase UINode { get; set; }

        public Type GetTypeUI() => typeof(MaterialParamAnimationEditor);

        public void OnLoadUI(object uiInstance) { }

        public void OnRenderUI(object uiInstance)
        {
          //  var editor = (MaterialParamAnimationEditor)uiInstance;
          //  editor.LoadEditor(this);
        }

        private ResDict<MaterialAnim> AnimDict;

        public BfresMaterialAnim() { }

        public BfresMaterialAnim(ResFile resFile, ResDict<MaterialAnim> dict, MaterialAnim anim, string name)
        {
            Root = new AnimationTree.AnimNode(this);
            ResFile = resFile;
            AnimDict = dict;
            ModelName = name;
            MaterialAnim = anim;
            UINode = new NodeBase(anim.Name) { Tag = this };
            UINode.CanRename = true;
            UINode.OnHeaderRenamed += delegate
            {
                //not changed
                if (anim.Name == UINode.Header)
                    return;

                //Dupe name
                if (AnimDict.ContainsKey(UINode.Header))
                {
                    TinyFileDialog.MessageBoxErrorOk($"Name {UINode.Header} already exists!");
                    //revert
                    UINode.Header = anim.Name;
                    return;
                }
                OnNameChanged(UINode.Header);
            };
            UINode.Icon = '\uf0e7'.ToString();

            CanPlay = false; //Default all animations to not play unless toggled in list
            Reload(anim);
        }

        public BfresMaterialAnim(MaterialAnim anim, string name) {
            ModelName = name;
            CanPlay = false; //Default all animations to not play unless toggled in list
            Reload(anim);
        }

        public void OnSave()
        {
            MaterialAnim.FrameCount = (int)this.FrameCount;
            MaterialAnim.Loop = this.Loop;

            int hash = BfresAnimations.CalculateGroupHashes(this);
            if (IsEdited || hash != Hash) //Generate anim data
            {
                this.TextureList = GetTextureList(this);
                MaterialAnimConverter.ConvertAnimation(this, MaterialAnim);
            }
            Hash = hash;
        }

        static List<string> GetTextureList(BfresMaterialAnim anim)
        {
            //Prepare an optimal texture list with only used textures
            List<string> textureList = new List<string>();

            foreach (STAnimGroup group in anim.AnimGroups)
            {
                foreach (var track in group.GetTracks())
                {
                    if (!(track is BfresMaterialAnim.SamplerTrack))
                        continue;

                    foreach (var key in track.KeyFrames)
                    {
                        var texture = anim.TextureList[(int)key.Value];
                        if (!textureList.Contains(texture))
                            textureList.Add(texture);

                        key.Value = textureList.IndexOf(texture);
                    }
                }
            }
            return textureList;
        }

        public void OnNameChanged(string newName)
        {
            string previousName = MaterialAnim.Name;
            MaterialAnim.Name = newName;

            if (AnimDict.ContainsKey(previousName))
            {
                AnimDict.RemoveKey(previousName);
                AnimDict.Add(MaterialAnim.Name, MaterialAnim);
            }
        }

        public void InsertParamKey(string material, ShaderParam param)
        {
            this.IsEdited = true;

            var group = this.AnimGroups.FirstOrDefault(x => x.Name == material);
            //Add new material group if doesn't exist
            if (group == null) {
                group = new MaterialAnimGroup() { Name = material, };
                this.AnimGroups.Add(group);
                //Add UI node
                if (!Root.Children.Any(x => x.Header == group.Name))
                    Root.AddChild(MaterialAnimUI.GetGroupNode(this, null, group));
            }
            var paramAnimGroup = (ParamAnimGroup)group.SubAnimGroups.FirstOrDefault(x => x.Name == param.Name);
            //Add new param group if doesn't exist
            if (paramAnimGroup == null) {
                paramAnimGroup = new ParamAnimGroup() { Name = param.Name, };
                group.SubAnimGroups.Add(paramAnimGroup);
            }
            //Insert key to param group
            paramAnimGroup.InsertParamKey(this.Frame, param);
            //Add UI node
            var matNode = Root.Children.FirstOrDefault(x => x.Header == group.Name);
            if (!matNode.Children.Any(x => x.Header == paramAnimGroup.Name))
                matNode.AddChild(MaterialAnimUI.CreateParamNodeHierachy(this, null, paramAnimGroup, group));
        }

        public void InsertTextureKey(string material, string sampler, string texture)
        {
            this.IsEdited = true;

            var group = (MaterialAnimGroup)this.AnimGroups.FirstOrDefault(x => x.Name == material);
            //Add new material group if doesn't exist
            if (group == null)
            {
                group = new MaterialAnimGroup() { Name = material, };
                this.AnimGroups.Add(group);
                //Add UI node
                if (!Root.Children.Any(x => x.Header == group.Name))
                    Root.AddChild(MaterialAnimUI.GetGroupNode(this, null, group));
            }
            var samplerTrack = (SamplerTrack)group.Tracks.FirstOrDefault(x => x.Name == sampler);
            //Add new sampler track if doesn't exist
            if (samplerTrack == null)
            {
                samplerTrack = new SamplerTrack() { Name = sampler, };
                samplerTrack.InterpolationType = STInterpoaltionType.Step;
                group.Tracks.Add(samplerTrack);
            }

            if (!TextureList.Contains(texture))
                TextureList.Add(texture);

            //Insert key to sampler track
            var keyFrame = samplerTrack.KeyFrames.FirstOrDefault(x => x.Frame == this.Frame);
            if (keyFrame == null)
                samplerTrack.Insert(new STKeyFrame(this.Frame, TextureList.IndexOf(texture)));
            else
                keyFrame.Value = TextureList.IndexOf(texture);
            //Add UI node
            var matNode = Root.Children.FirstOrDefault(x => x.Header == group.Name);
            if (!matNode.Children.Any(x => x.Header == samplerTrack.Name))
                matNode.AddChild(MaterialAnimUI.CreateSamplerNodeHierachy(this, samplerTrack, group));
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
            dlg.FileName = $"{MaterialAnim.Name}.bfmaa";
            dlg.AddFilter(".bfmaa", ".bfmaa");
            dlg.AddFilter(".json", ".json");

            if (dlg.ShowDialog()) {
                OnSave();
                MaterialAnim.Export(dlg.FilePath, ResFile);
            }
        }

        private void ReplaceAction()
        {
            var dlg = new ImguiFileDialog();
            dlg.FileName = $"{MaterialAnim.Name}.bfmaa";
            dlg.AddFilter(".bfmaa", ".bfmaa");
          //  dlg.AddFilter(".json", ".json");

            if (dlg.ShowDialog())
            {
                MaterialAnim.Import(dlg.FilePath, ResFile);
                Reload(MaterialAnim);
            }
        }

        private void DeleteAction()
        {
            int result = TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to remove these animations? Operation cannot be undone.");
            if (result != 1)
                return;

            UINode.Parent.Children.Remove(UINode);

            if (ResFile.ShaderParamAnims.ContainsValue(this.MaterialAnim))
                ResFile.ShaderParamAnims.Remove(this.MaterialAnim);
            if (ResFile.TexSrtAnims.ContainsValue(this.MaterialAnim))
                ResFile.TexSrtAnims.Remove(this.MaterialAnim);
            if (ResFile.ColorAnims.ContainsValue(this.MaterialAnim))
                ResFile.ColorAnims.Remove(this.MaterialAnim);
            if (ResFile.TexPatternAnims.ContainsValue(this.MaterialAnim))
                ResFile.TexPatternAnims.Remove(this.MaterialAnim);
        }

        public BfresMaterialAnim Clone() {
            BfresMaterialAnim anim = new BfresMaterialAnim();
            anim.Name = this.Name;
            anim.ModelName = this.ModelName;
            anim.FrameCount = this.FrameCount;
            anim.Frame = this.Frame;
            anim.Loop = this.Loop;
            anim.TextureList = this.TextureList;
            anim.AnimGroups = this.AnimGroups;
            return anim;
        }

        public List<Material> GetMaterialData(string name)
        {
            List<Material> materials = new List<Material>();
            foreach (var model in this.ResFile.Models.Values)
            {
                foreach (var mat in model.Materials.Values)
                {
                    if (mat.Name == name)
                        materials.Add(mat);

                }
            }
            return materials;
        }

        /// <summary>
        /// Gets the parent render that this animation belongs to.
        /// </summary>
        public BfresRender GetParentRender()
        {
            if (DataCache.ModelCache.ContainsKey(ModelName))
                return (BfresRender)DataCache.ModelCache[ModelName];

            return null;
        }

        public BfresMaterialRender[] GetMaterials()
        {
            List<BfresMaterialRender> materials = new List<BfresMaterialRender>();

            if (!DataCache.ModelCache.ContainsKey(ModelName))
                return null;

            var models = ((BfresRender)DataCache.ModelCache[ModelName]).Models;
            if (models.Count == 0) return null;

            if (!((BfresRender)DataCache.ModelCache[ModelName]).InFrustum)
                return null;

            foreach (var model in models)
            {
                if (model.IsVisible)
                {
                    foreach (BfresMeshRender mesh in model.MeshList)
                        materials.Add((BfresMaterialRender)mesh.MaterialAsset);
                }
            }
            return materials.ToArray();
        }

        public override void NextFrame()
        {
            var materials = GetMaterials();
            if (materials == null)
                return;

            int numMats = 0;
            foreach (var mat in materials)
            {
                foreach (MaterialAnimGroup group in AnimGroups)
                {
                    if (group.Name != mat.Name)
                        continue;

                    ParseAnimationTrack(group, mat);
                    numMats++;
                }
            }
            if (numMats > 0)
                MapStudio.UI.AnimationStats.MaterialAnims += 1;
        }

        private void ParseAnimationTrack(STAnimGroup group, BfresMaterialRender mat)
        {
            foreach (var track in group.GetTracks())
            {
                if (track is SamplerTrack)
                    ParseSamplerTrack(mat, (SamplerTrack)track);
                if (track is ParamTrack)
                    ParseParamTrack(mat, group, (ParamTrack)track);
            }

            foreach (var subGroup in group.SubAnimGroups)
                ParseAnimationTrack(subGroup, mat);
        }

        private void ParseSamplerTrack(BfresMaterialRender material, SamplerTrack track)
        {
            if (TextureList.Count == 0)
                return;

            var value = (int)track.GetFrameValue(this.Frame);
            if (TextureList.Count > value)
            {
                var texture = TextureList[value];
                if (texture != null)
                {
                    if (material.Material.AnimatedSamplers.ContainsKey(track.Name))
                        material.Material.AnimatedSamplers[track.Name] = texture;
                    else
                        material.Material.AnimatedSamplers.Add(track.Name, texture);
                }
            }
        }

        private void ParseParamTrack(BfresMaterialRender matRender, STAnimGroup group, ParamTrack track)
        {
            if (!matRender.Material.ShaderParams.ContainsKey(group.Name))
                return;

            if (matRender is BfshaRenderer)
                ((BfshaRenderer)matRender).UpdateMaterialBlock = true; 

            var value = track.GetFrameValue(this.Frame);

            //4 bytes per float or int value
            uint index = track.ValueOffset / 4;

            var targetParam = matRender.Material.ShaderParams[group.Name];

            var param = new ShaderParam();

            if (!matRender.Material.AnimatedParams.ContainsKey(group.Name)) {
                if (targetParam.DataValue is float[]) {
                    float[] values = (float[])targetParam.DataValue;
                    float[] dest = new float[values.Length];
                    Array.Copy(values, dest, values.Length);
                    param.DataValue = dest;
                }
                else
                    param.DataValue = targetParam.DataValue;

                param.Type = targetParam.Type;
                param.Name = group.Name;

                matRender.Material.AnimatedParams.Add(group.Name, param);
            }

            if (!matRender.Material.AnimatedParams.ContainsKey(group.Name))
                return;

            param = matRender.Material.AnimatedParams[group.Name];

            switch (targetParam.Type)
            {
                case ShaderParamType.Float: param.DataValue = (float)value; break;
                case ShaderParamType.Float2:
                case ShaderParamType.Float3:
                case ShaderParamType.Float4:
                    ((float[])param.DataValue)[index] = value;
                    break;
                case ShaderParamType.Int: param.DataValue = value; break;
                case ShaderParamType.Int2:
                case ShaderParamType.Int3:
                case ShaderParamType.Int4:
                    ((int[])param.DataValue)[index] = (int)value;
                    break;
                case ShaderParamType.TexSrt:
                case ShaderParamType.TexSrtEx:
                    {
                        TexSrtMode mode = ((TexSrt)param.DataValue).Mode;
                        var translateX = ((TexSrt)param.DataValue).Translation.X;
                        var translateY = ((TexSrt)param.DataValue).Translation.Y;
                        var rotate = ((TexSrt)param.DataValue).Rotation;
                        var scaleX = ((TexSrt)param.DataValue).Scaling.X;
                        var scaleY = ((TexSrt)param.DataValue).Scaling.Y;

                       // if (track.ValueOffset == 0) mode = (TexSrtMode)Convert.ToUInt32(value);
                        if (track.ValueOffset == 4) scaleX = value;
                        if (track.ValueOffset == 8) scaleY = value;
                        if (track.ValueOffset == 12) rotate = value;
                        if (track.ValueOffset == 16) translateX = value;
                        if (track.ValueOffset == 20) translateY = value;

                        param.DataValue = new TexSrt()
                        {
                            Mode = mode,
                            Scaling = new Syroot.Maths.Vector2F(scaleX, scaleY),
                            Translation = new Syroot.Maths.Vector2F(translateX, translateY),
                            Rotation = rotate,
                        };
                    }
                    break;
                case ShaderParamType.Srt2D:
                    {
                        var translateX = ((Srt2D)param.DataValue).Translation.X;
                        var translateY = ((Srt2D)param.DataValue).Translation.Y;
                        var rotate = ((Srt2D)param.DataValue).Rotation;
                        var scaleX = ((Srt2D)param.DataValue).Scaling.X;
                        var scaleY = ((Srt2D)param.DataValue).Scaling.Y;

                        if (track.ValueOffset == 0) scaleX = value;
                        if (track.ValueOffset == 4) scaleY = value;
                        if (track.ValueOffset == 8) rotate = value;
                        if (track.ValueOffset == 12) translateX = value;
                        if (track.ValueOffset == 16) translateY = value;

                        param.DataValue = new Srt2D()
                        {
                            Scaling = new Syroot.Maths.Vector2F(scaleX, scaleY),
                            Translation = new Syroot.Maths.Vector2F(translateX, translateY),
                            Rotation = rotate,
                        };
                    }
                    break;
            }
        }

        public void Reload(MaterialAnim anim)
        {
            Name = anim.Name;
            FrameCount = anim.FrameCount;
            FrameRate = 60.0f;
            Loop = anim.Loop;
            if (anim.TextureNames != null)
                TextureList = anim.TextureNames.Keys.ToList();

            if (anim.MaterialAnimDataList == null)
                return;

            AnimGroups.Clear();
            foreach (var matAnim in anim.MaterialAnimDataList) {
                var group = new MaterialAnimGroup();
                AnimGroups.Add(group);
                group.Name = matAnim.Name;

                //Get the material animation's texture pattern animation lists
                //Each sampler has their own info
                for (int i = 0; i < matAnim.PatternAnimInfos.Count; i++)
                {
                    var patternInfo = matAnim.PatternAnimInfos[i];

                    //Get the curve index for animated indices
                    int curveIndex = patternInfo.CurveIndex;
                    //Get the base index for starting values
                    int textureBaseIndex = matAnim.BaseDataList.Length > i ? matAnim.BaseDataList[i] : 0;

                    //Make a new sampler track using step interpolation
                    var samplerTrack = new SamplerTrack();
                    samplerTrack.InterpolationType = STInterpoaltionType.Step;
                    samplerTrack.Name = patternInfo.Name;
                    group.Tracks.Add(samplerTrack);

                    if (curveIndex != -1)
                        BfresAnimations.GenerateKeys(samplerTrack, matAnim.Curves[curveIndex], true);
                    else //Use the base data and make a constant key
                        samplerTrack.KeyFrames.Add(new STKeyFrame(0, textureBaseIndex));
                }
                //Get the list of animated parameters
                for (int i = 0; i < matAnim.ParamAnimInfos.Count; i++)
                {
                    ParamAnimGroup paramGroup = new ParamAnimGroup();
                    paramGroup.Name = matAnim.ParamAnimInfos[i].Name;
                    group.SubAnimGroups.Add(paramGroup);

                    var paramInfo = matAnim.ParamAnimInfos[i];
                    //Params have int and float curves
                    int curveIndex = paramInfo.BeginCurve;
                    int constantIndex = paramInfo.BeginConstant;
                    int numFloats = paramInfo.FloatCurveCount;
                    int numInts = paramInfo.IntCurveCount;
                    int numConstants = paramInfo.ConstantCount;

                    //Each constant and curve get's their own value using a value offset
                    for (int j = 0; j < numConstants; j++) {
                        if (constantIndex + j >= matAnim.Constants.Count)
                            continue;

                        var constant = matAnim.Constants[constantIndex + j];
                        float value = constant.Value;
                        bool isInit = constant.Value.Int32 > 0 && constant.Value.Int32 < 6;
                        //A bit hacky, convert int32 types by value range SRT modes use
                        if (isInit)
                            value = constant.Value.Int32;

                        paramGroup.Tracks.Add(new ParamTrack()
                        {
                            Name = constant.AnimDataOffset.ToString("X"),
                            ValueOffset = constant.AnimDataOffset,
                            //Not the best way, but 4 is typically the stride size for each value
                            ChannelIndex = (int)(constant.AnimDataOffset / 4),
                            KeyFrames = new List<STKeyFrame>() { new STKeyFrame(0, value) },
                            InterpolationType = STInterpoaltionType.Constant,
                            IsInt32 = isInit,
                        });
                    }
                    //Loop through all int and float curve values
                    for (int j = 0; j < numInts + numFloats; j++)
                    {
                        var curve = matAnim.Curves[curveIndex + j];
                        var paramTrack = new ParamTrack() { Name = curve.AnimDataOffset.ToString("X") };
                        paramTrack.ValueOffset = curve.AnimDataOffset;
                        //Not the best way, but 4 is typically the stride size for each value
                        paramTrack.ChannelIndex = (int)(curve.AnimDataOffset / 4);
                        if (numInts > 0)
                            paramTrack.IsInt32 = j < numInts;
                        paramGroup.Tracks.Add(paramTrack);

                        BfresAnimations.GenerateKeys(paramTrack, curve);
                    }
                }
            }

            if (ResFile != null)
                MaterialAnimUI.ReloadTree(Root, this, ResFile);

            Hash = BfresAnimations.CalculateGroupHashes(this);
        }

        public class MaterialAnimGroup : STAnimGroup
        {
            public List<STAnimationTrack> Tracks = new List<STAnimationTrack>();

            public override List<STAnimationTrack> GetTracks() { return Tracks; }

            public MaterialAnimGroup Clone()
            {
                var matGroup = new MaterialAnimGroup();
                matGroup.Name = this.Name;
                matGroup.Category = this.Category;
                foreach (var group in this.SubAnimGroups)
                {
                    if (group is ICloneable)
                        matGroup.SubAnimGroups.Add(((ICloneable)group).Clone() as STAnimGroup);
                }
                foreach (var track in this.Tracks)
                {
                    if (track is ICloneable)
                        matGroup.Tracks.Add(((ICloneable)track).Clone() as STAnimationTrack);
                }
                return matGroup;
            }
        }

        public class ParamAnimGroup : STAnimGroup, ICloneable
        {
            public List<STAnimationTrack> Tracks = new List<STAnimationTrack>();

            public override List<STAnimationTrack> GetTracks() { return Tracks; }

            public object Clone()
            {
                var paramGroup = new ParamAnimGroup();
                paramGroup.Name = this.Name;
                foreach (BfresAnimationTrack track in this.Tracks)
                    paramGroup.Tracks.Add((STAnimationTrack)track.Clone());
                return paramGroup;
            }

            public void RemoveKey(float frame)
            {
                foreach (var track in Tracks) {
                    track.RemoveKey(frame);
                }
            }

            public void InsertParamKey(float frame, ShaderParam param)
            {
                //Insert all possible track types
                switch (param.Type)
                {
                    case ShaderParamType.Float:
                        InsertKey(frame, 0, (float)param.DataValue, "Value");
                        break;
                    case ShaderParamType.Float2:
                    case ShaderParamType.Float3:
                    case ShaderParamType.Float4:
                        var values = (float[])param.DataValue;
                        string[] tracks = new string[4] { "X", "Y", "Z", "W" };
                        for (int i = 0; i < values.Length; i++)
                            InsertKey(frame, i * sizeof(float), values[i], tracks[i]);
                        break;
                    case ShaderParamType.TexSrt:
                    case ShaderParamType.TexSrtEx:
                        InsertKey(frame, 4, ((TexSrt)param.DataValue).Scaling.X, "Scale.X");
                        InsertKey(frame, 8, ((TexSrt)param.DataValue).Scaling.Y, "Scale.Y");
                        InsertKey(frame, 12, ((TexSrt)param.DataValue).Rotation, "Rotation");
                        InsertKey(frame, 16, ((TexSrt)param.DataValue).Translation.X, "Position.X");
                        InsertKey(frame, 20, ((TexSrt)param.DataValue).Translation.Y, "Position.Y");
                        break;
                }
            }

            public void InsertKey(float frame, int offset, float value, string trackName) {
                InsertKey(frame, offset, value, 0, 0, trackName);
            }

            public void InsertKey(float frame, int offset, float value) {
                InsertKey(frame, offset, value, 0 , 0, offset.ToString("X"));
            }

            public void InsertKey(float frame, int offset, float value, float slopeIn, float slopeOut, string trackName)
            {
                var interpolation = STInterpoaltionType.Linear;
                //Deternine what other tracks might be using and use that instead
                if (Tracks.Count > 0)
                    interpolation = Tracks.FirstOrDefault().InterpolationType;

                var editedTrack = Tracks.FirstOrDefault(x => ((ParamTrack)x).ValueOffset == offset);
                if (editedTrack == null)
                {
                    editedTrack = new ParamTrack()
                    {
                        ValueOffset = (uint)offset,
                        Name = trackName,
                        InterpolationType = interpolation
                    };
                    Tracks.Add(editedTrack);
                }

                if (editedTrack.InterpolationType == STInterpoaltionType.Hermite)
                {
                    editedTrack.Insert(new STHermiteKeyFrame()
                    {
                        Frame = frame,
                        Value = value,
                        TangentIn = slopeIn,
                        TangentOut = slopeOut,
                    });
                }
                else if (editedTrack.InterpolationType == STInterpoaltionType.Linear)
                {
                    editedTrack.Insert(new STKeyFrame()
                    {
                        Frame = frame,
                        Value = value,
                    });
                }
                else if (editedTrack.InterpolationType == STInterpoaltionType.Step)
                {
                    editedTrack.Insert(new STKeyFrame()
                    {
                        Frame = frame,
                        Value = value,
                    });
                }
            }
        }

        public class SamplerTrack : BfresAnimationTrack
        {
            public override object Clone()
            {
                var track = new SamplerTrack();
                this.Clone(track);
                return track;
            }
        }

        public class ParamTrack : BfresAnimationTrack
        {
            /// <summary>
            /// The offset value of the value offset in byte length.
            /// </summary>
            public uint ValueOffset { get; set; } 

            public bool IsInt32 { get; set; }

            public ParamTrack() { }

            public ParamTrack(uint offset, float value, string name)
            {
                ValueOffset = offset;
                this.KeyFrames.Add(new STKeyFrame(0, value));
                Name = name;
            }

            public override object Clone()
            {
                var track = new ParamTrack();
                track.ValueOffset = this.ValueOffset;
                this.Clone(track);
                return track;
            }
        }
    }
}
