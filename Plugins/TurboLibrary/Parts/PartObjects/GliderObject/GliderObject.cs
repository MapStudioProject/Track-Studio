using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartsLibrary;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;
using TurboLibrary.Parts;

namespace PartsEditor
{
    public  class GliderObject : PartObject
    {
        /// <summary>
        /// Gets or sets the scale of the glider.
        /// </summary>
        [PartParam(SectionIdentifier.Glider)]
        public float Scale { get; set; }

        [PartParam(SectionIdentifier.Glider)]
        internal int RecolorFlags { get; set; }

        public bool HasRecolors
        {
            get { return RecolorFlags == 1; }
            set {
                if (value)
                    RecolorFlags = 1;
                else
                    RecolorFlags = 0;
            }
        }

        private int WingID;

        public GliderObject(int wingID, string partName)
            : base("", partName)
        {
            WingID = wingID;
        }

        public GliderDriverObject GetDriver(EDriverID driver)
        {
            return new GliderDriverObject(WingID, (int)driver);
        }
    }
}
