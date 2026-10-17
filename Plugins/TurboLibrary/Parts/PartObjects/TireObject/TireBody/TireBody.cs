using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor.TireBody
{
    public class TireBody : PartObject
    {
        public string Name
        {
            get { return MushResParts.KartNameList[BodyIndex]; }
        }

        [PartParam(SectionIdentifier.TireBody)]
        public float ScaleX { get; set; }

        [PartParam(SectionIdentifier.TireBody)]
        public float ScaleYZ { get; set; }

        private int _flags;

        [PartParam(SectionIdentifier.TireBody)]
        public int Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        //Override flags
        public bool UseTireBodyArmFront
        {
            get => (_flags & 1) == 0;
            set => FlagUtil.SetFlag(ref _flags, 1, !value);
        }

        public bool UseTireBodyArmBack
        {
            get => (_flags & 2) == 0;
            set => FlagUtil.SetFlag(ref _flags, 2, !value);
        }

        public bool UseTireBodySusOffsetFront
        {
            get => (_flags & 4) == 0;
            set => FlagUtil.SetFlag(ref _flags, 4, !value);
        }

        public bool UseTireBodySusOffsetBack
        {
            get => (_flags & 8) == 0;
            set => FlagUtil.SetFlag(ref _flags, 8, !value);
        }


        [PartParam(SectionIdentifier.TireBody)]
        public TireShaftPointScale ArmFront { get; set; }

        [PartParam(SectionIdentifier.TireBody)]
        public TireShaftPointScale ArmBack { get; set; }

        [PartParam(SectionIdentifier.TireBody)]
        public Vector3F SusArmOffsetFront { get; set; }

        [PartParam(SectionIdentifier.TireBody)]
        public Vector3F SusArmOffsetBack { get; set; }

        private int BodyIndex { get; set; }
        private int TireIndex { get; set; }


        public TireBody(int tireIndex, int bodyIndex)
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
            return data[TireIndex][BodyIndex][index];
        }

        internal override void SetValue(SectionIdentifier type, int index, Dword value)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            data[TireIndex][BodyIndex][index] = value;
        }
    }
}
