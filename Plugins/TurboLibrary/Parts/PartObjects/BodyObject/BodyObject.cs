using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PartsLibrary;
using TurboLibrary.Parts;
using TurboLibrary;

namespace PartsEditor
{
    public class BodyObject : PartObject
    {
        public bool IsATV => this.Type == BodyType.ATV;
        public bool IsBike => this.Type == BodyType.OutsideBike;

        [PartParam(SectionIdentifier.DriverEffect, 0)]
        public Vector2S EffectFlags { get; set; }

        [PartParam(SectionIdentifier.Body, 0)]
        internal Vector2S BodyFlags { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BodyType Type
        {
            get { return (BodyType)BodyFlags.X; }
            set { BodyFlags = new Vector2S((short)value, BodyFlags.Y); }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public WheelAmount WheelType
        {
            get { return (WheelAmount)BodyFlags.Y; }
            set { BodyFlags = new Vector2S(BodyFlags.X, (short)value); }
        }

        /// <summary>
        /// Angles the body's rotation from the handle's location.
        /// Used to slightly tilt the angles for drivers on certain bikes.
        /// </summary>
        [PartParam(SectionIdentifier.Body)]
        public float SeatAngle { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector2F GliderOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector2F AntennaOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public WaterPropellerTransform WaterPropellerTransform { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public GearBoxTransform GearBoxTransform { get; set; }

        [PartParam(SectionIdentifier.Body)]
        internal Vector4B SusFlags { get; set; } //Front, Back type

        public byte SusTypeFront
        {
            get { return SusFlags.X; }
            set { SusFlags.X = value; }
        }

        public byte SusTypeBack
        {
            get { return SusFlags.Y; }
            set { SusFlags.Y = value; }
        }

        [PartParam(SectionIdentifier.Body)]
        public float SusLengthFront { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float SusLengthBack { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F FrontLightOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float HeadlightAngle { get; set; }
        /// <summary>
        /// Max steering angle in degrees. 
        /// </summary>
        [PartParam(SectionIdentifier.Body)]
        public float MaxSteeringAngle { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float PackunOffsetZ { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F BalloonOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float AnimMoveSpeedScale { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float LOD0Threshold { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public int Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private int _flags;

        public bool IsCoverLeg
        {
            get => ((_flags << 0x1a) >> 0x1f) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 0x1a, value);
        }

        public bool IsHeadLightSingle
        {
            get => ((_flags << 0x1b) >> 0x1f) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 0x1b, value);
        }

        public bool IsNormalArmWhenAntiG
        {
            get => ((_flags << 0x1c) >> 0x1f) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 0x1c, value);
        }

        public bool IsExternalHeadLight
        {
            get => ((_flags << 0x1e) >> 0x1f) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 0x1e, value);
        }

        public bool IsEmissionRythm
        {
            get => ((_flags << 0x19) >> 0x1f) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 0x19, value);
        }

        public bool IsHeadHandleLocal
        {
            get => ((byte)(_flags >> 7)) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 7, value);
        }

        [PartParam(SectionIdentifier.Body)]
        public TireShaftTransform TireShaftTransform { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector2F ParamBG { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector2F ParamBI { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F SusBodyOffsetFront { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F SusBodyOffsetBack { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public TireKartTransform TireKartTransform { get; set; }

        private int BodyIndex;

        public BodyTireObject GetTire(ETireID tire)
        {
            return new BodyTireObject(BodyIndex, (int)tire);
        }

        public BodyGliderObject GetWing(EWingID wing)
        {
            return new BodyGliderObject(BodyIndex, (int)wing);
        }

        public BodyDriverObject GetDriver(EDriverID driver)
        {
            return new BodyDriverObject(BodyIndex, (int)driver);
        }

        public BodyObject(int index, string partName)
            : base("", partName)
        {
            BodyIndex = index;
        }

        public enum BodyType
        {
            Kart = 0,
            OutsideBike = 1,
            InsideBike = 2,
            ATV = 3,
        }

        public enum WheelAmount
        {
            FourWheels,
            TwoWheels,
            ThreeWheels,
        }
    }
}
