using MapStudio.UI;
using OpenTK;
using PartsEditor;
using PartsEditor.TireBody;
using PartsLibrary;
using SharpEXR.AttributeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary.Parts
{
    public class KartParameter
    {
        //IDs to assign for parameter loading
        public EDriverID DriverID;
        public ETireID TireID;
        public EWingID WingID;
        public EBodyID BodyID;

        //Objects of the parts
        public DriverObject DriverPart;
        public BodyObject BodyPart;
        public TireObject TirePart;
        public GliderObject WingPart;

        public BodyTireObject BodyTire;
        public BodyGliderObject BodyWing;
        public BodyDriverObject BodyDriver;

        public TireBody TireBody;

        public GliderDriverObject WingDriver;

        public int TireBodyRemapID;

        public bool IsBike => BodyPart.BodyFlags.X != 0;
        public bool IsBikeRideType => BodyPart.BodyFlags.X == 1 || BodyPart.BodyFlags.Y == 3;
        public bool IsTrike => BodyPart.BodyFlags.X == 2;
        public bool IsTrikeR => BodyPart.BodyFlags.X == 3;
        public bool IsHangOnBike => BodyPart.BodyFlags.Y == 2;
        public bool IsATVRideType => BodyPart.BodyFlags.Y == 3;
        public bool HasGearbox => BodyTire.HasGearbox;
        public bool HasFender => BodyTire.HasFender;

        //Todo figure out where this is set
        public int NumFrontTires = 2;
        public int NumBackTires = 2;

        //Todo these get set by glider driver flags
        public bool IsBigWing = false;
        public bool IsBigFlag = false;

        //Todo figure out where this is set
        public bool HasArmTexture => false;

        //Kart parameters

        public Vector2F WingOffset;
        public Vector2F AntennaOffset;
        public Vector3F BalloonOffset;

        public float WingScale => WingDriver.Scale;

        public float ScrewScale0;
        public float ScrewScale1;

        public Vector3F ScrewOffset0;
        public Vector3F ScrewOffset1;

        public Vector3F GearScale;
        public Vector3F GearFrontOffset;
        public Vector3F GearBackOffset;

        public Vector3F ShaftFrontScale;
        public Vector3F ShaftBackScale;
        public Vector3F ShaftFrontOffset;
        public Vector3F ShaftBackOffset;
        public Vector3F ShaftFrontAntiGravityOffset;
        public Vector3F ShaftBackAntiGravityOffset;

        public Vector3F ArmPointFrontPos;
        public Vector3F ArmPointBackPos;

        public Vector3F ArmPointFrontAntiGravityPos;
        public Vector3F ArmPointBackAntiGravityPos;

        //Sus
        public int SusFrontType;
        public int SusBackType;

        public float SusFrontLength;
        public float SusBackLength;

        public Vector3F SusArmOffsetFront;
        public Vector3F SusArmOffsetBack;

        public Vector3F SusBodyOffsetFront;
        public Vector3F SusBodyOffsetBack;

        //Tire
        public Vector3F TireFrontScale;
        public Vector3F TireBackScale;

        public Vector3F TireFrontOffset;
        public Vector3F TireBackOffset;
        public Vector3F TireFrontAntiGravityOffset;
        public Vector3F TireBackAntiGravityOffset;

        public float TireRotOffsetX;
        public float TireRotAntiGravityOffsetX;

        public float TireCamberRateFront;
        public float TireCamberRateBack;
        public float TireCamberRateAntiG;

        //Lights
        public Vector3F HeadlightOffset;
        public float HeadlightAngle;

        //Extra
        public float PackunOffsetZ;
        public float LOD0Threshold;
        public float AnimMoveSpeedScale;

        //Todo figure these out
        public float GetSpringRate() => 1.0f;
        public float GetSpringLengthNormal() => 1.0f;
        public float GetSpringLengthMax() => 1.0f;

        public Vector3 GetWheelAxisLocalVecFront(bool isRightSide) {
            return new Vector3(0, 0, isRightSide ? 1f : -1f);
        }

        public Vector3 GetWheelAxisLocalVecBack(bool isRightSide) {
            return new Vector3(0, 0, isRightSide ? 1f : -1f);
        }

        public void SetupParts()
        {
            MushResParts.TireIndex = (int)WingID;
            MushResParts.BodyIndex = (int)TireID;
            MushResParts.GliderIndex = (int)BodyID;
            MushResParts.DriverIndex = (int)DriverID;

            //Get all parts
            DriverPart = MushResParts.GetDriver(DriverID);
            BodyPart = MushResParts.GetBody(BodyID);
            TirePart = MushResParts.GetTire(TireID);
            WingPart = MushResParts.GetWing(WingID);

            //Tire part overrides
            TireBodyRemapID = TirePart.GetRemapID().BodyTireIndex;
            TireBody = TirePart.GetBody(BodyID);

            //Body part overrides
            BodyTire = BodyPart.GetTire((ETireID)TireBodyRemapID);
            BodyDriver = BodyPart.GetDriver(DriverID);
            BodyWing = BodyPart.GetWing(WingID);


            //Wing part overrides
            WingDriver = WingPart.GetDriver(DriverID);

            bool overrideGliderOffset = (BodyWing.Flags & 1) == 0;
            bool overrideAntennaOffset = (BodyWing.Flags & 2) == 0;

            //Glider
            WingOffset = overrideGliderOffset ? BodyWing.GliderOffset : BodyPart.GliderOffset;
            AntennaOffset = overrideAntennaOffset ? BodyWing.AntennaOffset : BodyPart.AntennaOffset;

            //Water fan
            ScrewScale0 = BodyPart.WaterPropellerTransform.ScewScale;
            ScrewScale1 = BodyPart.WaterPropellerTransform.FanScale;
            ScrewOffset0 = new Vector3F(0,
                 BodyPart.WaterPropellerTransform.ScewOffsetYZ.X,
                 BodyPart.WaterPropellerTransform.ScewOffsetYZ.Y);
            ScrewOffset1 = new Vector3F(0,
                 BodyPart.WaterPropellerTransform.FanOffsetYZ.X,
                 BodyPart.WaterPropellerTransform.FanOffsetYZ.Y);

            //Balloon
            BalloonOffset = BodyPart.BalloonOffset;

            //Gear box
            GearScale = BodyPart.GearBoxTransform.Scale;
            GearFrontOffset = BodyPart.GearBoxTransform.FrontOffset;
            GearBackOffset = BodyPart.GearBoxTransform.BackOffset;

            //Lights
            HeadlightOffset = BodyPart.FrontLightOffset;
            HeadlightAngle = BodyPart.HeadlightAngle;

            //PackunOffsetZ
            PackunOffsetZ = BodyPart.PackunOffsetZ;

            LOD0Threshold = BodyPart.LOD0Threshold;
            AnimMoveSpeedScale = BodyPart.AnimMoveSpeedScale;

            //Tire  
            TireFrontScale = new Vector3F(
                BodyTire.TireScaleX,
                BodyTire.TireScaleYZ, 
                BodyTire.TireScaleYZ);

            TireBackScale = TireFrontScale * new Vector3F(
                TireBody.ScaleX,
                TireBody.ScaleYZ,
                TireBody.ScaleYZ);

            TireRotOffsetX = TirePart.TireRotOffsetX;
            TireRotAntiGravityOffsetX = TirePart.TireRotOffsetXAntiG;

            if (BodyTire.UseTireCamberRateFront) TireCamberRateFront = TirePart.TireCamberRateFront;
            if (BodyTire.UseTireCamberRateBack) TireCamberRateBack = TirePart.TireCamberRateBack;
            if (BodyTire.UseTireCamberRateAntiG) TireCamberRateAntiG = TirePart.TireCamberRateAntiG;

            float GetValue(float a, float b, bool useA) {
                return useA ? a : b;
            }

            Vector3F GetVec3(Vector3F a, Vector3F b, bool useA) {
                return useA ? a : b;
            }

            Vector3F GetScale(float aX, float xYZ, float bX, float bYZ, bool useA) {
                return useA ? new Vector3F(aX, xYZ, xYZ) : new Vector3F(bX, bYZ, bYZ);
            }

            //Tire camber overrides
            TireCamberRateFront = GetValue(TirePart.TireCamberRateFront, BodyTire.TireCamberRateFront, BodyTire.UseTireCamberRateFront);
            TireCamberRateBack = GetValue(TirePart.TireCamberRateBack, BodyTire.TireCamberRateBack, BodyTire.UseTireCamberRateBack);
            TireCamberRateAntiG = GetValue(TirePart.TireCamberRateAntiG, BodyTire.TireCamberRateAntiG, BodyTire.UseTireCamberRateAntiG);

            //Shaft scale overrides
            ShaftFrontScale = GetScale(
                BodyTire.TireShaftTransform.FrontScaleX,
                BodyTire.TireShaftTransform.FrontScaleYZ,
                BodyPart.TireShaftTransform.FrontScaleX,
                BodyPart.TireShaftTransform.FrontScaleYZ, BodyTire.UseBodyTireShaftFrontScale);

            ShaftBackScale = GetScale(
                BodyTire.TireShaftTransform.BackScaleX,
                BodyTire.TireShaftTransform.BackScaleYZ,
                BodyPart.TireShaftTransform.BackScaleX,
                BodyPart.TireShaftTransform.BackScaleYZ, BodyTire.UseBodyTireShaftBackScale);

            //Shaft offset
            ShaftFrontOffset = GetVec3(
                BodyTire.TireShaftTransform.FrontOffset,
                BodyPart.TireShaftTransform.FrontOffset, BodyTire.UseBodyTireShaftFrontOffset);

            ShaftBackOffset = GetVec3(
             BodyTire.TireShaftTransform.BackOffset,
             BodyPart.TireShaftTransform.BackOffset, BodyTire.UseBodyTireShaftBackOffset);

            //Shaft offset anti g
            ShaftFrontAntiGravityOffset = GetVec3(
             BodyTire.TireShaftTransform.FrontAntiGravityOffset,
             BodyPart.TireShaftTransform.FrontAntiGravityOffset, BodyTire.UseBodyTireKartAntiGFrontTransform);

            ShaftBackAntiGravityOffset = GetVec3(
             BodyTire.TireShaftTransform.BackAntiGravityOffset,
             BodyPart.TireShaftTransform.BackAntiGravityOffset, BodyTire.UseBodyTireKartAntiGBackTransform);

            //Tire offsets
            TireFrontOffset = GetVec3(
                BodyTire.TireKartTransform.FrontOffset,
                BodyPart.TireKartTransform.FrontOffset,
                BodyTire.UseBodyTireKartFrontTransform);
            TireBackOffset = GetVec3(
                BodyTire.TireKartTransform.BackOffset,
                BodyPart.TireKartTransform.BackOffset,
                BodyTire.UseBodyTireKartBackTransform);

            //Tire offsets anti g
            TireFrontAntiGravityOffset = GetVec3(
                BodyTire.TireKartTransform.FrontAntiGravityOffset,
                BodyPart.TireKartTransform.FrontAntiGravityOffset,
                BodyTire.UseBodyTireKartAntiGFrontTransform);
            TireBackAntiGravityOffset = GetVec3(
                BodyTire.TireKartTransform.BackAntiGravityOffset,
                BodyPart.TireKartTransform.BackAntiGravityOffset,
                BodyTire.UseBodyTireKartAntiGBackTransform);

            //Arm points
            ArmPointFrontPos = GetVec3(
                TireBody.ArmFront.ArmPointPos,
                TirePart.ArmFront.ArmPointPos,
                TireBody.UseTireBodyArmFront);

            ArmPointBackPos = GetVec3(
                TireBody.ArmBack.ArmPointPos,
                TirePart.ArmBack.ArmPointPos,
                TireBody.UseTireBodyArmBack);

            //Sus params
            SusFrontType = BodyPart.SusTypeFront;
            SusBackType = BodyPart.SusTypeBack;
            SusFrontLength = BodyPart.SusLengthFront;
            SusBackLength = BodyPart.SusLengthBack;

            //Sus arms
            SusArmOffsetFront = GetVec3(
                TireBody.SusArmOffsetFront,
                TirePart.SusArmOffsetFront,
                TireBody.UseTireBodySusOffsetFront);

            SusArmOffsetBack = GetVec3(
                TireBody.SusArmOffsetBack,
                TirePart.SusArmOffsetBack,
                TireBody.UseTireBodySusOffsetBack);

            //Sus body
            SusBodyOffsetFront = GetVec3(
                BodyTire.SusBodyOffsetFront,
                BodyPart.SusBodyOffsetFront,
                BodyTire.UseBodyTireSusFrontOffset);

            SusBodyOffsetBack = GetVec3(
                BodyTire.SusBodyOffsetBack,
                BodyPart.SusBodyOffsetBack,
                BodyTire.UseBodyTireSusBackOffset);
        }

        public void SavePartsBin()
        {
            if (!Directory.Exists(GlobalSettings.ModOutputPath))
            {
                TinyFileDialog.MessageBoxInfoOk("Select a mod path to save the parts.bin");

                ImguiFolderDialog dlg = new ImguiFolderDialog();
                if (dlg.ShowDialog())
                {
                    GlobalSettings.ModOutputPath = dlg.SelectedPath;
                    PluginConfig.Instance.Save();
                }
            }
            string modPath = GlobalSettings.ModOutputPath;
            if (!Directory.Exists(modPath))
                return;

            string path = System.IO.Path.Combine(modPath, "Common", "Mush", "Parts.bin");
            PartsEditor.MushResParts.BinFile.Save(path);

            TinyFileDialog.MessageBoxInfoOk($"Saved parts.bin to {path}!");
        }
    }
}
