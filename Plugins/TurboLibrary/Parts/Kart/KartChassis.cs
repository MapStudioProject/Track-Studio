using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary;
using OpenTK;
using Toolbox.Core;

namespace TurboLibrary.Parts.Kart
{
    public class KartChassis
    {
        public STBone BodyRoot;

        //Gearbox
        private STBone GearFrontBone;
        private STBone GearBackBone;

        //Water fan
        private STBone ScrewBone;
        private STBone FanBone;

        //Bike 
        public STBone HandleBone;
        public STBone ArmBBone;

        public KartChassis(KartVehicle vehicle, bool hasGearbox, bool isBigWing, bool isBigFlag)
        {
            {
                var idx = vehicle.BodyModelIdx;
                var skeleton = vehicle.ResKartBody.ModelFolder.Models[idx].Skeleton;
                BodyRoot = skeleton.Bones.FirstOrDefault();

                HandleBone = skeleton.SearchBone("Jnt_Handle");
                ArmBBone = skeleton.SearchBone("Jnt_ArmB");
            }

            //Gearbox
            if (hasGearbox)
            {
                var skeleton = vehicle.ResGear.ModelFolder.Models[0].Skeleton;
                GearFrontBone = skeleton.SearchBone("Gear");
                GearBackBone = skeleton.SearchBone("Slide");
            }
            //Water fan
            {
                var skeleton = vehicle.ResScrewFan.ModelFolder.Models[0].Skeleton;
                ScrewBone = skeleton.SearchBone("Screw");
                FanBone = skeleton.SearchBone("Rotate");
            }
        }

        public void UpdateBody(KartVehicle vehicle, float scale)
        {
            BodyRoot.Position = vehicle.KartOffset;
            BodyRoot.Scale = new Vector3(scale);

            BodyRoot.Skeleton.Reset();
        }

        public void UpdateGearBox(KartVehicle vehicle, KTransform gearFront, KTransform gearBack)
        {
            GearFrontBone.Position = gearFront.Position;
            GearFrontBone.Scale = gearFront.Scale;
            GearBackBone.Position = gearBack.Position;
            GearBackBone.Scale = gearBack.Scale;

            GearFrontBone.Skeleton.Reset();
        }

        public void UpdateFan(KTransform screw, KTransform fan)
        {
            ScrewBone.Position = screw.Position;
            ScrewBone.Scale = screw.Scale;
            FanBone.Position = fan.Position;
            FanBone.Scale = fan.Scale;

            ScrewBone.Skeleton.Reset();
        }
    }

    public class KartBalloon : KTransform
    {
    }

    public class KartHeadLight
    {
    }
}
