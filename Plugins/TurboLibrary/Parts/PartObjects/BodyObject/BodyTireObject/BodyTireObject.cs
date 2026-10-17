using PartsEditor;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;
using System;

namespace PartsLibrary
{
    public class BodyTireObject : PartObject
    {
        public string Name
        {
            get { return MushResParts.TireNameList[TireIndex]; }
        }

        [PartParam(SectionIdentifier.BodyTire, 0)]
        public float TireScaleX { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public float TireScaleYZ { get; set; }

        private int _flags;

        [PartParam(SectionIdentifier.BodyTire)]
        public int Flags
        {
            get => _flags;
            set { _flags = value; }
        }

        //Tire section flags
        public bool UseTireCamberRateFront
        {
            get => (Flags & 0x2000) != 0;
            set => FlagUtil.SetFlag(ref _flags, 0x2000, value);
        }

        public bool UseTireCamberRateBack
        {
            get => (Flags & 0x4000) != 0;
            set => FlagUtil.SetFlag(ref _flags, 0x4000, value);
        }

        public bool UseTireCamberRateAntiG
        {
            get => (Flags & 0x8000) != 0;
            set => FlagUtil.SetFlag(ref _flags, 0x8000, value);
        }

        //Body section flags

        public bool HasGearbox
        {
            get => ((Flags << 0x13) >> 0x1f) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 0x13, value);
        }

        public bool HasFender
        {
            get => (Flags & 1) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1, value);
        }

        public bool IsNormalArmWhenAntiG
        {
            get => ((Flags << 0x1c) >> 0x1f) != 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 0x1c, value);
        }

        private int ShafFlag => _flags & 0xFF;
        private int KartFlag => (_flags >> 8) & 0xFF;

        public bool UseBodyTireShaftFrontScale
        {
            get => (ShafFlag & 1) == 0;
            set => FlagUtil.SetFlag(ref _flags, 1, !value);
        }

        public bool UseBodyTireShaftBackScale
        {
            get => (ShafFlag & 2) == 0;
            set => FlagUtil.SetFlag(ref _flags, 2, !value);
        }

        public bool UseBodyTireShaftFrontOffset
        {
            get => (ShafFlag & 4) == 0;
            set => FlagUtil.SetFlag(ref _flags, 4, !value);
        }

        public bool UseBodyTireShaftBackOffset
        {
            get => (ShafFlag & 8) == 0;
            set => FlagUtil.SetFlag(ref _flags, 8, !value);
        }

        public bool UseBodyTireParamBG
        {
            get => (ShafFlag & 0x10) == 0;
            set => FlagUtil.SetFlag(ref _flags, 0x10, !value);
        }

        public bool UseBodyTireParamBI
        {
            get => (ShafFlag & 0x20) == 0;
            set => FlagUtil.SetFlag(ref _flags, 0x20, !value);
        }

        public bool UseBodyTireSusBackOffset
        {
            get => (ShafFlag & 0x40) == 0;
            set => FlagUtil.SetFlag(ref _flags, 0x40, !value);
        }

        public bool UseBodyTireSusFrontOffset
        {
            get => (ShafFlag & 0x80) == 0;
            set => FlagUtil.SetFlag(ref _flags, 0x80, !value);
        }

        public bool UseBodyTireKartFrontTransform
        {
            get => (KartFlag & 1) == 0;
            set => FlagUtil.SetFlag(ref _flags, 1 << 8, !value);
        }

        public bool UseBodyTireKartBackTransform
        {
            get => (KartFlag & 2) == 0;
            set => FlagUtil.SetFlag(ref _flags, 2 << 8, !value);
        }

        public bool UseBodyTireKartAntiGFrontTransform
        {
            get => (KartFlag & 4) == 0;
            set => FlagUtil.SetFlag(ref _flags, 4 << 8, !value);
        }

        public bool UseBodyTireKartAntiGBackTransform
        {
            get => (KartFlag & 8) == 0;
            set => FlagUtil.SetFlag(ref _flags, 8 << 8, !value);
        }

        [PartParam(SectionIdentifier.BodyTire)]
        public PartsEditor.BodyTire.TireShaftTransform TireShaftTransform { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public Vector2F ParamBG { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public Vector2F ParamBI { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public Vector3F SusBodyOffsetFront { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public Vector3F SusBodyOffsetBack { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public PartsEditor.BodyTire.TireKartTransform TireKartTransform { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public float TireCamberRateFront { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public float TireCamberRateBack { get; set; }

        [PartParam(SectionIdentifier.BodyTire)]
        public float TireCamberRateAntiG { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int TireIndex { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int BodyIndex { get; set; }

        public BodyTireObject(int bodyIndex, int tireIndex)
        {
            TireIndex = tireIndex;
            BodyIndex = bodyIndex;
            UpdateGetters();
        }

        internal override Dword GetValue(SectionIdentifier type, int index)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            return data[BodyIndex][TireIndex][index];
        }

        internal override void SetValue(SectionIdentifier type, int index, Dword value)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            data[BodyIndex][TireIndex][index] = value;
        }
    }
}
