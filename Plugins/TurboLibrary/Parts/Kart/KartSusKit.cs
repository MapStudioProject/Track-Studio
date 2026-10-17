using BfresLibrary;
using CafeLibrary;
using GLFrameworkEngine;
using OpenTK;
using PartsEditor;
using SharpEXR.AttributeTypes;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;

namespace TurboLibrary.Parts.Kart
{
    public class KartSusKit
    {
        public List<KartTire> Tires = new List<KartTire>();

        public List<KartSuspension> Arms = new List<KartSuspension>();

        public void Create(KartVehicle vehicle, int numFrontTires, int numBackTires, SusType susTypeFront, SusType susTypeBack,
            bool hasArmTexture, bool normalAntiGArm)
        {
            Tires.Clear();
            Arms.Clear();

            //Create suspensions for each tire
            for (int i = 0; i < numFrontTires; i++)
            {
                bool isSingle = numFrontTires == 1;
                bool isRightSide = i == 0; //right (0) or left (1)

                Arms.Add(new KartSuspension(vehicle, susTypeFront, true, isSingle, isRightSide, normalAntiGArm));
                Tires.Add(new KartTire(vehicle, susTypeFront, true, isSingle, isRightSide));
            }

            for (int i = 0; i < numBackTires; i++)
            {
                bool isSingle = numBackTires == 1;
                bool isRightSide = i == 0; //right (0) or left (1)

                Arms.Add(new KartSuspension(vehicle, susTypeBack, false, isSingle, isRightSide, normalAntiGArm));
                Tires.Add(new KartTire(vehicle, susTypeBack, false, isSingle, isRightSide));
            }
        }

        public void SetParameter(KartVehicle vehicle, KartParameter kartParameter, float global_scale = 1.0f)
        {
            for (int i = 0; i < Tires.Count; i++)
            {
                float springRate = kartParameter.GetSpringRate() * global_scale;
                float springLength = kartParameter.GetSpringLengthNormal() * global_scale;
                float springLengthMax = kartParameter.GetSpringLengthMax() * global_scale;

                {
                    Vector3F tirescale = new Vector3F(1f);
                    Vector3F offset = new Vector3F(0f);

                    float offsetRotX = STMath.Lerp(
                            kartParameter.TireRotOffsetX,
                            kartParameter.TireRotAntiGravityOffsetX, vehicle.AntiGravityRatio);

                    offsetRotX *= global_scale;

                    float camberRate = 0f;
                    Vector3 axisRotation = kartParameter.GetWheelAxisLocalVecBack(Tires[i].IsRightside);
                    if (Tires[i].IsFront)
                        axisRotation = kartParameter.GetWheelAxisLocalVecFront(Tires[i].IsRightside);

                    //This seems to get scaled
                    axisRotation *= global_scale;

                    if (Tires[i].IsFront)
                    {
                        camberRate = STMath.Lerp(
                            kartParameter.TireCamberRateFront,
                            kartParameter.TireCamberRateAntiG, vehicle.AntiGravityRatio);

                        offset = Vector3F.Lerp(
                            kartParameter.TireFrontOffset,
                            kartParameter.TireFrontAntiGravityOffset, vehicle.AntiGravityRatio);

                        tirescale = kartParameter.TireFrontScale;
                    }
                    else
                    {
                        camberRate = STMath.Lerp(
                            kartParameter.TireCamberRateBack,
                            kartParameter.TireCamberRateAntiG, vehicle.AntiGravityRatio);

                        offset = Vector3F.Lerp(
                            kartParameter.TireBackOffset,
                            kartParameter.TireBackAntiGravityOffset, vehicle.AntiGravityRatio);

                        tirescale = kartParameter.TireBackScale;
                    }

                    float angle = MathF.PI - camberRate;
                    float antiGAngle = kartParameter.TireCamberRateAntiG * 1.570796f;

                    //Lerp angles
                    angle = STMath.Lerp(angle, antiGAngle, vehicle.AntiGravityRatio);

                    float halfRadius = kartParameter.TirePart.TireColRadius / 2f;
                    float hoverPlus = kartParameter.TirePart.TireCollRadiusHoverPlus;

                    float height = STMath.Lerp(0, -((halfRadius + hoverPlus)), vehicle.AntiGravityRatio);
                    offset += new Vector3F(0, height, 0);

                    Tires[i].CalculateWheelAxleRot(axisRotation, angle, offsetRotX);
                    Tires[i].SetBaseScale(tirescale, global_scale);
                    Tires[i].Offset = offset * new Vector3F(global_scale);

                    //Set collision
                    Tires[i].CollisionWidthHalf = kartParameter.TirePart.TireWidthHalf;
                    Tires[i].CollisionRadius = kartParameter.TirePart.TireColRadius;
                }

                //Setup arm
                {

                    if (!Tires[i].IsFront)
                    {
                        Arms[i].Offset = Vector3F.Lerp(
                            kartParameter.ShaftBackOffset,
                            kartParameter.ShaftBackAntiGravityOffset,
                            vehicle.AntiGravityRatio) * new Vector3F(global_scale);

                        Arms[i].SetScale(kartParameter.ShaftFrontScale, global_scale);
                        Arms[i].ArmPointPos = kartParameter.ArmPointFrontPos;
                        Arms[i].SusArmOffset = kartParameter.SusArmOffsetFront;
                        Arms[i].SusBodyOffset = kartParameter.SusBodyOffsetFront;
                        Arms[i].SusLength = kartParameter.SusFrontLength;
                    }
                    else
                    {
                        Arms[i].Offset = Vector3F.Lerp(
                            kartParameter.ShaftFrontOffset,
                            kartParameter.ShaftFrontAntiGravityOffset,
                            vehicle.AntiGravityRatio) * new Vector3F(global_scale);

                        Arms[i].SetScale(kartParameter.ShaftBackScale, global_scale);
                        Arms[i].ArmPointPos = kartParameter.ArmPointBackPos;
                        Arms[i].SusArmOffset = kartParameter.SusArmOffsetBack;
                        Arms[i].SusBodyOffset = kartParameter.SusBodyOffsetBack;
                        Arms[i].SusLength = kartParameter.SusBackLength;
                    }
                }
            }
        }

        public void CalcApply(KartVehicle vehicle)
        {
            for (int i = 0; i < Tires.Count; i++)
                Tires[i].CalcApply(vehicle, Arms[i]);
            for (int i = 0; i < Arms.Count; i++)
                Arms[i].CalcApply(vehicle, Tires[i]);
        }

        public class KartTire : KTransform
        {
            private STBone Bone;

            public bool IsRightside;
            public bool IsFront;
            public bool IsSingle;

            public float CollisionWidthHalf;
            public float CollisionRadius;

            public Vector3F Offset;
            public float OffsetRotX;

            public KartTire(KartVehicle vehicle, SusType susType, bool isFront, bool isSingle, bool isRightSide)
            {
                //Determine the tire bone to target
                var skeleton = vehicle.ResKartTire.ModelFolder.Models[0].Skeleton;

                IsRightside = isRightSide;
                IsFront = isFront;
                IsSingle = isSingle;

                string type = IsFront ? "F" : "B";
                string side = isRightSide ? "R" : "L";

                //reversed
                if (vehicle.IsTrikeR)
                    type = IsFront ? "B" : "F";

                if (isSingle)
                    Bone = skeleton.SearchBoneSuffix($"_{type}");
                else
                    Bone = skeleton.SearchBoneSuffix($"_{side}{type}");

                Console.WriteLine();
            }

            public void CalculateWheelAxleRot(Vector3 vec, float angle, float rotOffsetX)
            {
                Rotation = Quaternion.FromAxisAngle(vec, angle);
                OffsetRotX = rotOffsetX;
            }

            public void SetBaseScale(Vector3F scale, float baseScale)
            {
                Scale = new Vector3(scale.X, scale.Y, scale.Z) * baseScale;
            }

            public void SetBone(string boneName, STSkeleton skeleton)
            {
                Bone = skeleton.SearchBone(boneName);
                if (Bone == null)
                    throw new Exception($"Failed to find bone {boneName}!");
            }

            public void CalcApply(KartVehicle vehicle, KartSuspension kartSuspension)
            {
                switch (kartSuspension.SusType)
                {
                    case SusType.Kart: 
                        CalcKartTire(vehicle);
                        break;
                    case SusType.BikeA:
                    case SusType.BikeA2:
                        CalcBikeATire(vehicle);
                        break;
                    case SusType.BikeB:
                    case SusType.BikeB2:
                        CalcBikeBTire(vehicle);
                        break;
                    default:
                        CalcKartTire(vehicle);
                        break;
                 }

                //Set transform output
                Bone.Position = this.Position;
                Bone.Scale = this.Scale;
                Bone.Rotation = this.Rotation;

                //Update bone
                Bone.Skeleton.Reset();
            }

            private void CalcBikeBTire(KartVehicle vehicle)
            {
                Position = new Vector3(Offset.X, Offset.Y, Offset.Z);
                Position.Y -= 2.3f;

                if (!IsRightside)
                    Position.X = -Position.X;

                //Shift kart offset
                Position += vehicle.KartOffset;
            }

            private void CalcKartTire(KartVehicle vehicle)
            {
                Position = new Vector3(Offset.X, Offset.Y, Offset.Z);

                if (!IsSingle)
                {
                    Position += new Vector3(OffsetRotX, 0, 0);

                    //Todo figure out how this is shifted
                    Position.X += 1f;
                }

                Position.Y -= 2.3f;

                if (!IsRightside)
                    Position.X = -Position.X;

                //Shift kart offset
                Position += vehicle.KartOffset;
            }

            private void CalcBikeATire(KartVehicle vehicle)
            {
                if (vehicle.KartChassis.HandleBone == null)
                    return;

                //Calculate from the handle placement
                var handlePos = vehicle.KartOffset + vehicle.KartChassis.HandleBone.Position;
                var handleRot = vehicle.KartChassis.HandleBone.Rotation;

                //Shift by rotation
                var localY = Vector3.Transform(Vector3.UnitY, handleRot);

                //Update tire position
                float y = -this.Offset.Z;
                var newPosition = localY * Vector3.Dot(new Vector3(0, y, 0), localY);
                this.Position = handlePos + newPosition;
            }

            private CylinderRenderer CylinderColDrawer;

            public void DrawCollisionDebug()
            {
                if (CylinderColDrawer == null)
                    CylinderColDrawer = new CylinderRenderer(1, 0.5f);

                Matrix4 scaleWidth = Matrix4.CreateScale(new Vector3(
                    CollisionWidthHalf * 2, 
                    CollisionRadius,
                    CollisionRadius));

                StandardMaterial mat = new StandardMaterial();
                mat.ModelMatrix =  Matrix4.CreateRotationZ(90 * STMath.Deg2Rad) * scaleWidth * Bone.Transform;
                mat.Color = new Vector4(1, 0, 0, 0.5f);
                mat.Render(GLContext.ActiveContext);

                GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();
                GLMaterialBlendState.TranslucentAlphaOne.RenderDepthTest();

                CylinderColDrawer.Draw(GLContext.ActiveContext);

                GLMaterialBlendState.Opaque.RenderBlendState();
                GLMaterialBlendState.Opaque.RenderDepthTest();
            }
        }

        public class KartSuspension : KTransform
        {
            private STBone ArmBone;
            private STBone SusBone;
            private STBone ArmAntiGBone;

            public SusType SusType;

            public bool IsRightside;
            public bool IsFront;
            public bool IsSingle;
            public bool IsNormalAntiGArm;

            //Arm data
            public Vector3F Offset;
            public Vector3F ArmPointPos;

            public Vector3F SusArmOffset;
            public Vector3F SusBodyOffset;
            public float SusLength;

            public KartSuspension(KartVehicle vehicle, SusType susType, bool isFront, 
                bool isSingle, bool isRightSide, bool normalAntiGArm)
            {
                CreateArm(vehicle, susType, isFront, isSingle, isRightSide, normalAntiGArm);
            }

            public void SetArmVisible(KartVehicle vehicle)
            {
                if (IsNormalAntiGArm)
                    return;

                var modelIdx = vehicle.ArmModelIdx;
                var modelAntiGIdx = vehicle.ArmModelAntiGIdx;
                var modelList = vehicle.GetArmResource(IsSingle).Renderer.Models;

                bool isAntiG = vehicle.AntiGravityRatio > 0.5f;

                modelList[modelIdx].IsVisible = !isAntiG;
                modelList[modelAntiGIdx].IsVisible = isAntiG;
            }

            public void CreateArm(KartVehicle vehicle, SusType susType, bool isFront, bool isSingle,
                bool isRightSide, bool normalAntiGArm)
            {
                IsRightside = isRightSide;
                IsFront = isFront;
                IsSingle = isSingle;
                IsNormalAntiGArm = normalAntiGArm;

                bool isBike = vehicle.IsBike;

                //Determine the arm bone to target
                {
                    var modelIdx = vehicle.ArmModelIdx;
                    var skeleton = vehicle.GetArmResource(IsSingle).ModelFolder.Models[modelIdx].Skeleton;

                    SusType = susType;

                    if (isSingle)
                    {
                        ArmBone = isFront ? skeleton.SearchBoneSuffix("_F") : skeleton.SearchBoneSuffix("_B");
                    }
                    else
                    {
                        if (isRightSide)
                            ArmBone = isFront ? skeleton.SearchBoneSuffix("_RF") : skeleton.SearchBoneSuffix("_RB");
                        else
                            ArmBone = isFront ? skeleton.SearchBoneSuffix("_LF") : skeleton.SearchBoneSuffix("_LB");
                    }

                    if (ArmBone == null)
                        throw new Exception($"Failed to find bone!");

                    switch (susType)
                    {
                        case SusType.Kart:
                            break;
                        case SusType.BikeA:
                        case SusType.BikeA2:
                            break;
                        case SusType.BikeB:
                           // SetArmBone(isFront ? "Jnt_ArmF" : "Jnt_ArmB", skeleton);
                            break;
                        case SusType.BikeB2:
                            //SetSusBone(isFront ? "Jnt_SusF" : "Jnt_SusB", skeleton);
                            //SetArmBone(isFront ? "Jnt_ArmF" : "Jnt_ArmB", skeleton);
                            break;
                    }
                }

                {
                    var modelIdx = vehicle.ArmModelAntiGIdx;
                    var skeleton = vehicle.GetArmResource(IsSingle).ModelFolder.Models[modelIdx].Skeleton;

                    //anti g
                    switch (susType)
                    {
                        case SusType.Kart:
                            if (isBike)
                                ArmAntiGBone = ArmBone;
                            else if (isSingle)
                                SetArmAntiGBone(isFront ? "ArmK_G_R" : "ArmK_G_R", skeleton);
                            else if (isRightSide)
                                SetArmAntiGBone(isFront ? "ArmK_G_RF" : "ArmK_G_RB", skeleton);
                            else
                                SetArmAntiGBone(isFront ? "ArmK_G_LF" : "ArmK_G_LB", skeleton);
                            break;
                        default:
                            ArmAntiGBone = ArmBone;
                            break;
                    }

                    if (normalAntiGArm)
                        ArmAntiGBone = ArmBone;
                }
            }

            public void SetOffset(Vector3F offset)
            {
                Position = new Vector3(offset.X, offset.Y, offset.Z);

                if (!IsRightside)
                    Position.X = -Position.X;
            }

            public void SetScale(Vector3F scale, float baseScale)
            {
                Scale = new Vector3(scale.X, scale.Y, scale.Z) * baseScale;
            }

            public void CalcApply(KartVehicle vehicle, KartTire tire)
            {
                SetArmVisible(vehicle);

                switch (SusType)
                {
                    case SusType.Kart:
                        CalcKartArm(vehicle, tire);
                        break;
                    case SusType.BikeA:
                        CalcBikeATire(vehicle, tire);
                        break;
                    default:
                        CalcKartArm(vehicle, tire);
                        break;
                }

                //Set transform output
                ArmBone.Position = this.Position;
                ArmBone.Scale = this.Scale;
                ArmBone.Rotation = this.Rotation;
                ArmBone.Skeleton.Reset();

                //Set anti g transform output
                ArmAntiGBone.Position = this.Position;
                ArmAntiGBone.Scale = this.Scale;
                ArmAntiGBone.Rotation = this.Rotation;
                ArmAntiGBone.Skeleton.Reset();
            }

            private void CalcKartArm(KartVehicle vehicle, KartTire tire)
            {
                Position = new Vector3(Offset.X, Offset.Y, Offset.Z);
                //Shift kart offset
                Position += vehicle.KartOffset;
                if (!IsRightside)
                    Position.X = -Position.X;

                CalcRotationFromTire(vehicle, tire.Position);
            }

            private void CalcBikeATire(KartVehicle vehicle, KartTire tire)
            {
                if (IsFront)
                {
                    var handlePos = vehicle.KartOffset + vehicle.KartChassis.HandleBone.Position;
                    var handleRot = vehicle.KartChassis.HandleBone.Rotation;

                    //Shift by rotation
                    var localY = Vector3.Transform(Vector3.UnitY, handleRot);
                    var newPosition = localY * Vector3.Dot(new Vector3(0, this.Offset.Y, 0), localY);
                    Position = handlePos + newPosition;
                    Rotation = handleRot;
                }
            }

            private void CalcRotationFromTire(KartVehicle vehicle, Vector3 target)
            {
                Vector3 axis = IsRightside ? new Vector3(1, 0, 0) : new Vector3(1, 0, 0);

                bool isAntiG = vehicle.AntiGravityRatio > 0.5f && !IsNormalAntiGArm;
                if (isAntiG)
                {
                    Vector3 src = this.Position;
                    Vector3 dst = target;
                    Vector3 dir = Vector3.Normalize(dst - src);
                    Rotation = STMath.QuatRotationFromTo(axis, dir);
                }
                else
                {
                    Vector3 src = this.Position;
                    Vector3 dst = target;
                    Vector3 dir = Vector3.Normalize(dst - src);

                    Rotation = STMath.QuatRotationFromTo(axis, dir);
                }
            }


            public void SetArmBone(string boneName, STSkeleton skeleton)
            {
                ArmBone = skeleton.SearchBone(boneName);
                if (ArmBone == null)
                    throw new Exception($"Failed to find bone {boneName}!");
            }

            public void SetArmAntiGBone(string boneName, STSkeleton skeleton)
            {
                ArmAntiGBone = skeleton.SearchBone(boneName);
                if (ArmAntiGBone == null)
                    throw new Exception($"Failed to find bone {boneName}!");
            }

            public void SetSusBone(string boneName, STSkeleton skeleton)
            {
                SusBone = skeleton.SearchBone(boneName);
                if (SusBone == null)
                    throw new Exception($"Failed to find bone {boneName}!");
            }
        }
    }
}
