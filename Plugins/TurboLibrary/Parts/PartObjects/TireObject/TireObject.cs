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
    public  class TireObject : PartObject
    {
        //Tire placement is controlled by 3 sections
        //Tire (default parameters for tires)
        //Tire Body (adjusted tires to configure per body)
        //Body Tire (default offsets used per body)

        /// <summary>
        /// Gets or sets the spin speed of the wheel.
        /// </summary>
        [PartParam(SectionIdentifier.Tire)]
        public float TireRevolutionMax { get; set; }

        /// <summary>
        /// Gets or sets the height of the current body height used by this tire.
        /// </summary>
        [PartParam(SectionIdentifier.Tire)]
        public float TireColRadius { get; set; }

        /// <summary>
        /// Gets or sets the spring rate of the suspension used by this tire.
        /// </summary>
        [PartParam(SectionIdentifier.Tire)]
        public float TireWidthHalf { get; set; }

        /// <summary>
        /// The offset in the X axis  of the tire position relative to it's current position.
        /// </summary>
        [PartParam(SectionIdentifier.Tire)]
        public float TireRotOffsetX { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public float TireRotOffsetXAntiG { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public float TireCollRadiusHoverPlus { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public Vector2F UnknownParam { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        internal Vector4B TireFlags { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public TireShaftPointScale ArmFront { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public TireShaftPointScale ArmBack { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public Vector3F SusArmOffsetFront { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public Vector3F SusArmOffsetBack { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public float TireCamberRateFront { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public float TireCamberRateBack { get; set; }

        [PartParam(SectionIdentifier.Tire)]
        public float TireCamberRateAntiG { get; set; }

        private int TireIndex;

        public TireObject(int index, string partName)
            : base("", partName)
        {
            TireIndex = index;
        }

        public TireBody.TireBody GetBody(EBodyID body)
        {
            return new TireBody.TireBody(TireIndex, (int)body);
        }

        public TireRemap GetRemapID()
        {
            return new TireRemap(TireIndex);
        }
    }
}
