using CafeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Toolbox.Core.Runtime;
using Toolbox.Core.IO;
using MapStudio.UI;
using OpenTK;
using System.ComponentModel;
using Collada141;
using Toolbox.Core;
using System.IO;

namespace TurboLibrary.Parts.Kart
{
    public class KartVehicle
    {
        public KartParameter Parameters;

        public KTransform Screw = new KTransform();
        public KTransform Fan = new KTransform();
        public KTransform GearFront = new KTransform();
        public KTransform GearBack = new KTransform();

        public KartBalloon KartBalloon;

        public KartChassis KartChassis;
        public KartHeadLight KartHeadLight;
        public KartSusKit KartSusKit = new KartSusKit();

        //Resources
        public BFRES ResScrewFan;
        public BFRES ResGear;

        public BFRES ResKartBody;
        public BFRES ResKartTire;

        public BFRES ResKartArm;

        public BFRES GetArmResource(bool isSingle)
        {
            return (isSingle) ? ResKartBody : ResKartArm;
        }

        public bool IsBike; //0x85
        public bool IsBikeRideType; //0x86
        public bool IsTrike; //0x87
        public bool IsTrikeR; //0x88
        public bool IsHangOnBike; //0x89
        public bool IsATVRideType; //0x8a

        public int BodyModelIdx;
        public int ArmModelIdx;
        public int ArmModelAntiGIdx;

        public float AntiGravityRatio = 0.0f;

        public Vector3 KartOffset = Vector3.Zero;

        //events to check if resource was changed
        public EventHandler OnPartsUpdated;
        public EventHandler OnTireResourceUpdated;
        public EventHandler OnBodyResourceUpdated;
        public EventHandler OnArmResourceUpdated;
        public EventHandler OnKartTransform;

        private FileEditor _editor;

        public Matrix4 GetKartMatrix()
        {
            return KartChassis.BodyRoot.Transform;
        }

        public void LoadResources(FileEditor editor, KartParameter parameter)
        {
            _editor = editor;
            Parameters = parameter;

            //Load resources
            LoadCommonResource(ref ResGear, "Gear", "Gear.bfres");
            LoadCommonResource(ref ResScrewFan, "Screw", "Screw.bfres");

            ReloadBody(parameter);
            ReloadTire(parameter);

            Setup(parameter);
        }

        public void ReloadBody(KartParameter parameter) {
            LoadResource(ref ResKartBody, "Body", $"Body{parameter.BodyID}");

            OnBodyResourceUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void ReloadTire(KartParameter parameter) {

            string kind = "K_";
            if (parameter.IsBike) kind = "B_";
            if (parameter.IsTrike || parameter.IsTrikeR) kind = "T_";

            LoadResource(ref ResKartTire, "Tire", $"Tire{kind}{parameter.TireID}");
            LoadResource(ref ResKartArm, "Arm", $"ArmK_{parameter.TireID}");

            if (ResKartArm != null)
            {
                ResKartArm.Renderer.Models[0].IsVisible = false;
                ResKartArm.Renderer.Models[1].IsVisible = true;
            }

            OnTireResourceUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Setup(KartParameter parameter)
        {
            bool isNormalArmWhenAntiG = parameter.BodyTire.IsNormalArmWhenAntiG;
            bool hasArmTexture = parameter.HasArmTexture;
            int numTiresFront = parameter.NumFrontTires;
            int numTiresBack = parameter.NumBackTires;

            IsBike = parameter.IsBike;
            IsHangOnBike = parameter.IsHangOnBike;
            IsBikeRideType = parameter.IsBikeRideType;
            IsATVRideType = parameter.IsATVRideType;
            IsTrike = parameter.IsTrike;
            IsTrikeR = parameter.IsTrikeR;

            if (parameter.IsBike && !IsATVRideType)
            {
                numTiresBack = 1;
                numTiresFront = 1;
            }

            if (IsTrike)
            {
                numTiresBack = 1;
                numTiresFront = 2;
            }
            if (IsTrikeR)
            {
                numTiresBack = 2;
                numTiresFront = 1;
            }

            ArmModelIdx = 1;
            ArmModelAntiGIdx = 0;
            BodyModelIdx = 0;

            if (ResKartArm != null)
            {
                foreach (var model in ResKartArm.ModelFolder.Models)
                {
                    foreach (var bone in model.Skeleton.Bones)
                    {
                        if (bone.Name.EndsWith("B"))
                            bone.Visible = numTiresBack == 2;
                        if (bone.Name.EndsWith("F"))
                            bone.Visible = numTiresFront == 2;
                    }
                }
            }

            //Figure out what models to target
            if (IsBike && !IsATVRideType || IsTrike || IsTrikeR) //arm is inside the body szs
            {
                ArmModelIdx = 1;
                BodyModelIdx = 3;

                this.ResKartBody.Renderer.Models[0].IsVisible = false;
                this.ResKartBody.Renderer.Models[1].IsVisible = false;
                this.ResKartBody.Renderer.Models[2].IsVisible = false;

                this.ResKartBody.Renderer.Models[ArmModelIdx].IsVisible = true;
            }

            KartChassis = new KartChassis(this,
                parameter.HasGearbox,
                parameter.IsBigWing,
                parameter.IsBigFlag);

            KartHeadLight = new KartHeadLight();

            KartSusKit = new KartSusKit();
            KartSusKit.Create(this, numTiresFront, numTiresBack, 
                (SusType)parameter.SusFrontType, (SusType)parameter.SusBackType, hasArmTexture, isNormalArmWhenAntiG);

            KartBalloon = new KartBalloon();
            KartBalloon.Position = new Vector3(
                parameter.BalloonOffset.X,
                parameter.BalloonOffset.Y,
                parameter.BalloonOffset.Z);

            SetParam(parameter);

            this.OnKartTransform?.Invoke(this, EventArgs.Empty);
        }

        public void ReloadParameters()
        {
            Parameters.SetupParts();

            SetParam(this.Parameters);

            this.OnKartTransform?.Invoke(this, EventArgs.Empty);
        }

        public void SetParam(KartParameter parameter, bool useEvent = true)
        {
            float global_scale = 1.0f;

            //Body offset
            float height = STMath.Lerp(
                parameter.TirePart.TireColRadius * global_scale,
                (parameter.TirePart.TireColRadius + parameter.TirePart.TireCollRadiusHoverPlus) * global_scale,
                AntiGravityRatio);

            KartOffset = new Vector3(0, height, 0);
            KartChassis.UpdateBody(this, global_scale);

            this.ResGear.Renderer.IsVisible = parameter.HasGearbox;

            //Calculate gear
            if (parameter.HasGearbox)
            {
                var frontOffset = parameter.GearFrontOffset;
                var backOffset = parameter.GearBackOffset;
                var gearscale = parameter.GearScale;

                GearFront.Position = new Vector3(frontOffset.X, frontOffset.Y, frontOffset.Z) * global_scale;
                GearBack.Position = new Vector3(backOffset.X, backOffset.Y, backOffset.Z);
                GearFront.Scale = new Vector3(gearscale.X, gearscale.Y, gearscale.Z) * global_scale;
                GearBack.Scale = new Vector3(gearscale.X, gearscale.Y, gearscale.Z);

                GearFront.Position += KartOffset;

                KartChassis.UpdateGearBox(this, GearFront, GearBack);
            }
            //Calculate water fan screw
            {
                var screwscale = parameter.ScrewScale0;
                var offset = parameter.ScrewOffset0;
                Screw.Position = new Vector3(offset.X, offset.Y, offset.Z) * global_scale;
                Screw.Scale = new Vector3(screwscale) * global_scale;

                Screw.Position += KartOffset;
            }
            //Calculate water fan
            {
                var offset = parameter.ScrewOffset1;
                var fanscale = parameter.ScrewScale1;
                Fan.Position = new Vector3(offset.X, offset.Y, offset.Z);
                Fan.Scale = new Vector3(fanscale);
            }

            KartChassis.UpdateFan(Screw, Fan);
            //Calculate sus and tire data
            KartSusKit.SetParameter(this, parameter, global_scale);

            KartSusKit.CalcApply(this);

            if (useEvent)
                OnPartsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void DrawDebug()
        {
            foreach (var tire in KartSusKit.Tires)
                tire.DrawCollisionDebug();
        }

        private void LoadResource(ref BFRES bfres, string folder, string id)
        {
            if (bfres != null)
                _editor.RemoveRender(bfres.Renderer);

            bfres?.Dispose();

            string filePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("Kart", folder, $"{id}.szs"));
            if (!GlobalSettings.IsMK8D) //mk8u lowercase
                filePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("kart", folder.ToLower(), $"{id}.szs"));

            if (!File.Exists(filePath))
                return;

            //Load resource
            bfres = (BFRES)STFileLoader.OpenFileFormat(filePath);
            _editor.AddRender(bfres.Renderer);
        }

        private void LoadCommonResource(ref BFRES bfres, string folder, string name)
        {
            if (bfres != null)
                _editor.RemoveRender(bfres.Renderer);

            bfres?.Dispose();

            string filePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("RaceCommon", folder, name));
            if (!GlobalSettings.IsMK8D) //mk8u lowercase
                filePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("race_common", folder.ToLower(), name));

            if (!File.Exists(filePath))
                return;

            //Load resource
            bfres = (BFRES)STFileLoader.OpenFileFormat(filePath);
            _editor.AddRender(bfres.Renderer);
        }

        public void Dispose()
        {
            ResKartBody?.Dispose();
            ResKartTire?.Dispose();
            ResKartArm?.Dispose();
        }
    }
}
