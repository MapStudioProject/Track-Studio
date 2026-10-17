using PartsEditor;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;
using Newtonsoft.Json;

namespace PartsLibrary
{
    public class BodyGliderObject : PartObject
    {
        public string Name
        {
            get { return MushResParts.GliderNameList[GliderIndex]; }
        }

        [PartParam(SectionIdentifier.BodyGlider)]
        public Vector2F GliderOffset { get; set; }

        [PartParam(SectionIdentifier.BodyGlider)]
        public Vector2F AntennaOffset { get; set; }

        [PartParam(SectionIdentifier.BodyGlider)]
        public int Flags { get; set; }


        private int GliderIndex { get; set; }
        private int BodyIndex { get; set; }

        public BodyGliderObject(int bodyIndex, int gliderIndex) {
            GliderIndex = gliderIndex;
            BodyIndex = BodyIndex;
            UpdateGetters();
        }

        internal override Dword GetValue(SectionIdentifier type, int index)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            return data[BodyIndex][GliderIndex][index];
        }

        internal override void SetValue(SectionIdentifier type, int index, Dword value)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            data[BodyIndex][GliderIndex][index] = value;
        }
    }
}
