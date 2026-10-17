using PartsEditor;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsLibrary
{
    public class GliderDriverObject : PartObject
    {
        [PartParam(SectionIdentifier.GliderDriver)]
        public int WingFlags { get; set; }

        [PartParam(SectionIdentifier.GliderDriver)]
        public float Scale { get; set; }

        [PartParam(SectionIdentifier.GliderDriver)]
        internal Vector2S RecolorFlags { get; set; }

        [PartParam(SectionIdentifier.GliderDriver)]
        public int DriverFlags { get; set; }

        public short DiffuseTexture
        {
            get { return RecolorFlags.X; }
            set { RecolorFlags = new Vector2S(value, RecolorFlags.Y); }
        }

        public short SpecularTexture
        {
            get { return RecolorFlags.Y; }
            set { RecolorFlags = new Vector2S(RecolorFlags.X, value); }
        }

        public bool IsBig => (WingFlags | 2) != 2;

        private int DriverIndex { get; set; }

        private int GliderIndex { get; set; }

        public GliderDriverObject(int gliderIndex, int driverIndex) {
            GliderIndex = gliderIndex;
            DriverIndex = driverIndex;
            UpdateGetters();
        }

        internal override Dword GetValue(SectionIdentifier type, int index)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            return data[GliderIndex][DriverIndex][index];
        }

        internal override void SetValue(SectionIdentifier type, int index, Dword value)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            data[GliderIndex][DriverIndex][index] = value;
        }
    }
}
