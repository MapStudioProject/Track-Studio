using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafeLibrary.Rendering;
using GLFrameworkEngine;
using MapStudio.UI;
using UIFramework;
using Toolbox.Core.ViewModels;
using OpenTK;
using PartsEditor;
using PartsLibrary;
using TurboLibrary.Parts.Kart;
using TurboLibrary.Parts;
using Syroot.NintenTools.MarioKart8.BinData;
using Toolbox.Core;
using System.IO;

namespace TurboLibrary
{
    public class PartsEditorPlugin : FileEditor
    {
        KartVehicle Kart = new KartVehicle();

       // EffectLinkU.KartEffect KartEffectWiIU = new EffectLinkU.KartEffect();

        //gui
        private NodeBase _bodyNode;
        private PartEditorObject _tireFNode;
        private PartEditorObject _tireBNode;
        private PartEditorObject _tireShaftFNode;
        private PartEditorObject _tireShaftBNode;
        private PartEditorObject _gearFNode;
        private PartEditorObject _gearBNode;
        private PartEditorObject _waterFanNode;
        private PartEditorObject _waterScrewNode;

        public string[] Description => new string[] { "Parts Binary" };

        public string[] Extension => new string[] { ".bin" };

        public bool CanSave { get; set; } = true;
        public File_Info FileInfo { get; set; } = new File_Info();

        public override bool CreateNew(string menu_name = "PartsBin")
        {
            Root.Header = "Parts.bin";
            string binFilePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("Common", "Mush", "Parts.bin"));
            if (!GlobalSettings.IsMK8D) //mk8u lowercase
                binFilePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("common", "mush", "Parts.bin"));

            if (!File.Exists(binFilePath))
            {
                TinyFileDialog.MessageBoxErrorOk($"Could not find parts.bin file!");
                return false;
            }

            if (GlobalSettings.IsMK8D)
            {
              //  EffectLink.Init();
            }
           // else
            //    KartEffectWiIU.Init(this, Kart);

            //First make a backup copy
            string backupPath = System.IO.Path.Combine(Toolbox.Core.Runtime.ExecutableDir, "Backup");
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            if (!File.Exists(System.IO.Path.Combine(backupPath, "Parts.bin")))
                File.Copy(binFilePath, System.IO.Path.Combine(backupPath, "Parts.bin"));

            //Reload gui if renderer updates
            Kart.OnTireResourceUpdated += delegate
            {
                if (Kart.ResKartTire != null)
                {
                    ((PartEditorObject)_tireFNode).Reload((BfresRender)Kart.ResKartTire.Renderer);
                    ((PartEditorObject)_tireBNode).Reload((BfresRender)Kart.ResKartTire.Renderer);
                }
            };
            Kart.OnArmResourceUpdated += delegate
            {
                if (Kart.ResKartArm != null)
                {
                    ((PartEditorObject)_tireShaftFNode).Reload((BfresRender)Kart.ResKartArm.Renderer);
                    ((PartEditorObject)_tireShaftBNode).Reload((BfresRender)Kart.ResKartArm.Renderer);
                }
            };

            KartDebugDrawer debugDrawer = new KartDebugDrawer(Kart);
            AddRender(debugDrawer);

            List<string> frontWheelBones = new List<string>() { "LF", "RF", "F", };
            List<string> backWheelBones = new List<string>() { "BF", "BF", "B", };
            List<string> gearFBones = new List<string>() { "Gear", };
            List<string> gearBBones = new List<string>() { "Slide", };
            List<string> waterFanBones = new List<string>() { "Rotate", };
            List<string> waterScrewFanBones = new List<string>() { "Screw", };

            _bodyNode = new NodeBase("Body");
            _bodyNode.Icon = '\uf1ba'.ToString();
            _bodyNode.Tag = this;

            _tireFNode = new PartEditorObject(Kart, "Tires Front", '\uf140', frontWheelBones);
            _tireBNode = new PartEditorObject(Kart, "Tires Back", '\uf140', backWheelBones);
            _tireShaftFNode = new PartEditorObject(Kart, "Tires Shaft Front", '\uf5ac', frontWheelBones);
            _tireShaftBNode = new PartEditorObject(Kart, "Tires Shaft Back", '\uf5ac', backWheelBones);
            _gearFNode = new PartEditorObject(Kart, "Gear Front", '\uf0b0', gearFBones);
            _gearBNode = new PartEditorObject(Kart, "Gear Back", '\uf0b0', gearBBones);
            _waterScrewNode = new PartEditorObject(Kart, "Water Fan Screw", '\uf863', waterScrewFanBones);
            _waterFanNode = new PartEditorObject(Kart, "Water Fan Back", '\uf863', waterFanBones);

            PartsEditor.MushResParts.BinFile = new BinFile(binFilePath);

            var parameters = new Parts.KartParameter();
            parameters.SetupParts();

            Kart.LoadResources(this, parameters);
            Kart.SetParam(parameters);

            ReloadKart(Kart, parameters);

            ((PartEditorObject)_gearFNode).Reload((BfresRender)Kart.ResGear.Renderer);
            ((PartEditorObject)_gearBNode).Reload((BfresRender)Kart.ResGear.Renderer);

            ((PartEditorObject)_waterScrewNode).Reload((BfresRender)Kart.ResScrewFan.Renderer);
            ((PartEditorObject)_waterFanNode).Reload((BfresRender)Kart.ResScrewFan.Renderer);

            PartsUI PartsUI = new PartsUI();
            PartsUI.OnKartChanged += delegate
            {
                ReloadKart(Kart, parameters);
            };

            _bodyNode.TagUI.UIDrawer += delegate { PartsUI.RenderBodyParams(Kart, parameters); };
            _tireFNode.TagUI.UIDrawer += delegate { PartsUI.RenderTireParams(Kart, parameters); };
            _tireBNode.TagUI.UIDrawer += delegate { PartsUI.RenderTireParams(Kart, parameters); };

            _tireShaftFNode.TagUI.UIDrawer += delegate { PartsUI.RenderShaftParams(Kart, parameters); };
            _tireShaftBNode.TagUI.UIDrawer += delegate { PartsUI.RenderShaftParams(Kart, parameters); };

            _gearFNode.TagUI.UIDrawer += delegate { PartsUI.RenderGearParams(Kart, parameters); };
            _gearBNode.TagUI.UIDrawer += delegate { PartsUI.RenderGearParams(Kart, parameters); };

            _waterFanNode.TagUI.UIDrawer += delegate { PartsUI.RenderWaterPropellerParams(Kart, parameters); };
            _waterScrewNode.TagUI.UIDrawer += delegate { PartsUI.RenderWaterPropellerParams(Kart, parameters); };


            return true;
        }

        private void ReloadKart(KartVehicle kart, KartParameter parameters)
        {
            //Gizmo tools by attaching the offset and world space positions
            foreach (var tire in kart.KartSusKit.Tires)
            {
                PartObject bodyPart = parameters.BodyPart;

                if (tire.IsFront)
                    bodyPart = parameters.BodyTire.UseBodyTireKartFrontTransform ? parameters.BodyTire : bodyPart;
                else
                    bodyPart = parameters.BodyTire.UseBodyTireKartBackTransform ? parameters.BodyTire : bodyPart;

                if (tire.IsFront && tire.IsRightside)
                    _tireFNode.SetGizmo(bodyPart, tire, "TireKartTransform", "FrontOffset", PartEdit.TireF);
                else if (tire.IsRightside)
                    _tireBNode.SetGizmo(bodyPart, tire, "TireKartTransform", "BackOffset", PartEdit.TireB);
            }

            foreach (var arm in kart.KartSusKit.Arms)
            {
                PartObject bodyPart = parameters.BodyPart;

                if (arm.IsFront)
                    bodyPart = parameters.BodyTire.UseBodyTireShaftFrontOffset ? parameters.BodyTire : bodyPart;
                else
                    bodyPart = parameters.BodyTire.UseBodyTireShaftBackOffset ? parameters.BodyTire : bodyPart;

                if (arm.IsFront && arm.IsRightside)
                    _tireShaftFNode.SetGizmo(bodyPart, arm, "TireShaftTransform", "FrontOffset", PartEdit.ShaftF);
                else if (arm.IsRightside)
                    _tireShaftBNode.SetGizmo(bodyPart, arm, "TireShaftTransform", "BackOffset", PartEdit.ShaftB);
            }

            _gearBNode.SetGizmo(parameters.BodyPart, kart.GearBack,
                "GearBoxTransform", "BackOffset", PartEdit.GearB);

            _gearFNode.SetGizmo(parameters.BodyPart, kart.GearFront,
                "GearBoxTransform", "FrontOffset", PartEdit.GearF);

            _waterScrewNode.SetGizmo(parameters.BodyPart, kart.Screw,
                "WaterPropellerTransform", "ScewOffsetYZ", PartEdit.WaterFanScrew, true);

            _waterFanNode.SetGizmo(parameters.BodyPart, kart.Fan,
                "WaterPropellerTransform", "FanOffsetYZ", PartEdit.WaterFan, true);

            Root.Children.Clear();
            Root.AddChild(_bodyNode);
            Root.AddChild(_tireFNode);
            Root.AddChild(_tireBNode);
            Root.AddChild(_tireShaftFNode);
            Root.AddChild(_tireShaftBNode);

            if (parameters.HasGearbox)
            {
                Root.AddChild(_gearFNode);
                Root.AddChild(_gearBNode);
            }
            Root.AddChild(_waterScrewNode);
            Root.AddChild(_waterFanNode);

            if (GlobalSettings.IsMK8D)
            {
              /*  var usd = EffectLink.XLink.UserDataTable.GetUserData(parameters.BodyID.ToString());
                //effect gui
                foreach (var effect in usd.AssetCallTable)
                {
                    if (effect.IsContainer)
                        continue;

                    AddRender(TurboEffectUI.CreateEffectObject(Root, usd, effect.Name));
                }*/
            }
            else
            {
            //    KartEffectWiIU.Prepare(this, kart, parameters.BodyID);
            }
        }

        public enum PartEdit
        {
            WaterFanScrew,
            WaterFan,
            GearF,
            GearB,
            TireF,
            TireB,
            ShaftF,
            ShaftB,
        }

        public override List<DockWindow> PrepareDocks()
        {
            List<DockWindow> dockWindows = new List<DockWindow>();
            dockWindows.Add(Workspace.Outliner);
            dockWindows.Add(Workspace.ViewportWindow);
            dockWindows.Add(Workspace.PropertyWindow);
            return dockWindows;
        }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            return false; //only loadable with file -> new menu
        }

        public void Load(Stream stream)
        {
        }

        public void Save(Stream stream)
        {
            PartsEditor.MushResParts.BinFile.Save(stream);
        }

        class PartEditorObject : NodeBase
        {
            TransformableObject Gizmo;

            public KartVehicle Kart;

            //Meshes to display selection
            private List<BfresMeshRender> Meshes = new List<BfresMeshRender>();

            //Bones that will be transformed
            private List<string> TargetBones = new List<string>();

            BfresRender Render;

            //gizmo editing data
            PartObject Part;
            KTransform Transform;
            string PartData;
            string OffsetParamName;
            bool IsYZ = false;
            PartEdit PartEdit;

            public PartEditorObject(KartVehicle vehicle, string text, char icon, List<string> targetBones)
            {
                Gizmo = new TransformableObject(null);

                Kart = vehicle;
                Header = text;
                Icon = icon.ToString();
                TargetBones = targetBones;
                Tag = this;

                this.OnSelected += delegate
                {
                    OnSelection();
                };

                Kart.OnPartsUpdated += delegate
                {
                    if (Transform != null && IsSelected)
                    {
                        SetPosTransformGizmo();

                        var transformTools = GLContext.ActiveContext.TransformTools;
                        transformTools.UpdateOrigin();
                    }
                };
            }

            public void SetGizmo(PartObject part, KTransform transform, string dataName, string offsetParamName, PartEdit edit, bool isYZ = false)
            {
                Gizmo.Transform.TransformUpdated = null;

                Part = part;
                Transform = transform;
                PartData = dataName;
                OffsetParamName = offsetParamName;
                IsYZ = isYZ;
                PartEdit = edit;
            }

            public void Reload(BfresRender render)
            {
                Render = render;
                render.MeshPicking = true;
                render.CanSelect = true;

                Meshes.Clear();

                //Load meshes that are binded to the target bones
                foreach (var model in render.Models)
                {
                    foreach (BfresMeshRender mesh in model.MeshList)
                    {
                        var bone = model.ModelData.Skeleton.Bones[mesh.BoneIndex];
                        //check if bone is targeted
                        foreach (var target in TargetBones)
                        {
                            //Add the mesh if it is targeted for selecting
                            if (bone.Name.EndsWith(target))
                            {
                                this.Meshes.Add(mesh);
                                break;
                            }
                        }
                    }
                }

               // GLContext.ActiveContext.Scene.DisableSelectionEvent = true;
                GLContext.ActiveContext.PickingTools.OnObjectPicked += (o, e) =>
                {
                    foreach (var mesh in Meshes)
                    {
                        if (mesh == o)
                        {
                            foreach (var node in this.Parent.Children)
                                node.IsSelected = false;

                            this.IsSelected = true;
                            OnSelection();
                        }
                    }
                };
            }

            public virtual void OnSelection()
            {
                GLContext.ActiveContext.Scene.DeselectAll(GLContext.ActiveContext);
                if (Render == null)
                    return;

                Render.IsSelected = true;

                //Deselect all meshes
                foreach (var model in Render.Models)
                {
                    foreach (BfresMeshRender mesh in model.MeshList)
                        mesh.IsSelected = false;
                }
                //Select only the target meshes that will move
                foreach (var mesh in Meshes)
                    mesh.IsSelected = true;

                if (IsYZ)
                    SetTransformYZGizmo(Part, Transform, PartData, OffsetParamName);
                else
                    SetTransformXYZGizmo(Part, Transform, PartData, OffsetParamName);
            }

            public void SetPosTransformGizmo()
            {
                //Set the gizmo world space pos
                Gizmo.Transform.Position = Transform.Position;
                //Add parent positions to fan and gear
                if (this.PartEdit == PartEdit.WaterFan)
                    Gizmo.Transform.Position = Kart.Screw.Position + Transform.Position;
                if (this.PartEdit == PartEdit.GearB)
                    Gizmo.Transform.Position = Kart.GearFront.Position + Transform.Position;

                Gizmo.Transform.UpdateMatrix(true);
            }

            private void SetTransformXYZGizmo(PartObject part, KTransform transform, string dataName, string offsetParamName)
            {
                //Reset
                Gizmo.Transform.TransformUpdated = null;

                SetPosTransformGizmo();

                //Transform event
                Gizmo.Transform.TransformUpdated += delegate
                {
                    var v = part.GetType().GetProperty(dataName).GetValue(part);

                    //Get the offset from part bin data
                    var prop = v.GetType().GetProperty(offsetParamName);
                    Vector3F offset = (Vector3F)prop.GetValue(v);

                    //The difference between the offset and world position
                    Vector3 diff = transform.Position - new Vector3(
                          offset.X,
                          offset.Y,
                          offset.Z);

                    //Small hack. Shift parent positions to gear
                    if (this.PartEdit == PartEdit.GearB)
                        diff += Kart.GearFront.Position;

                    //Gizmo world space back to offset space
                    Vector3 pos = Gizmo.Transform.Position - diff;

                    //Set the property back
                    prop.SetValue(v, new Vector3F(pos.X, pos.Y, pos.Z));
                    //Update the part setters which update the raw file data
                    part.UpdateSetters();
                    //Reload and recalculate all the part data
                    Kart.Parameters.SetupParts();
                    Kart.SetParam(Kart.Parameters, false);

                    //Update parent instances 
                    if (Part is BodyTireObject) Part = Kart.Parameters.BodyTire;
                    if (Part is BodyObject) Part = Kart.Parameters.BodyPart;
                };
                //Activate the gizmo
                var transformTools = GLContext.ActiveContext.TransformTools;
                transformTools.InitAction(new List<ITransformableObject>()
                {
                    Gizmo,
                });
              //  transformTools.TransformSettings.UseX = true;
              //  transformTools.TransformSettings.UseY = true;
               // transformTools.TransformSettings.UseZ = true;
            }

            private void SetTransformYZGizmo(PartObject part, KTransform transform, string dataName, string offsetParamName)
            {
                //Reset
                Gizmo.Transform.TransformUpdated = null;

                SetPosTransformGizmo();

                //Transform event
                Gizmo.Transform.TransformUpdated += delegate
                {
                    var v = part.GetType().GetProperty(dataName).GetValue(part);

                    //Get the offset from part bin data
                    var prop = v.GetType().GetProperty(offsetParamName);
                    Vector2F offset = (Vector2F)prop.GetValue(v);

                    //The difference between the offset and world position
                    Vector3 diff = transform.Position - new Vector3(
                          0,
                          offset.X,
                          offset.Y);

                    //Shift parent positions to fan
                    if (this.PartEdit == PartEdit.WaterFan)
                        diff += Kart.Screw.Position;

                    //Gizmo world space back to offset space
                    Vector3 pos = Gizmo.Transform.Position - diff;
                    //Set the property back
                    prop.SetValue(v, new Vector2F(pos.Y, pos.Z));
                    //Update the part setters which update the raw file data
                    part.UpdateSetters();
                    //Reload and recalculate all the part data
                    Kart.Parameters.SetupParts();
                    Kart.SetParam(Kart.Parameters, false);

                    //Update parent instances 
                    if (Part is BodyTireObject) Part = Kart.Parameters.BodyTire;
                    if (Part is BodyObject) Part = Kart.Parameters.BodyPart;
                };
                //Activate the gizmo
                var transformTools = GLContext.ActiveContext.TransformTools;
                transformTools.InitAction(new List<ITransformableObject>()
                {
                    Gizmo,
                });
              //  transformTools.TransformSettings.UseX = false;
              //  transformTools.TransformSettings.UseY = true;
              //  transformTools.TransformSettings.UseZ = true;
            }
        }
    }
}
