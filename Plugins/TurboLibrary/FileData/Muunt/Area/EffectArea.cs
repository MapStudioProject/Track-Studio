using Toolbox.Core;
using System;
using System.Collections.Generic;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an effect area controlling visual effects inside of it.
    /// </summary>
    [ByamlObject]
    public class EffectArea : PrmObject, ICloneable
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the effect flags of the effect played in this area.
        /// </summary>
        [ByamlMember]
        public EffectSWFlags EffectSW { get; set; }

        [Flags]
        public enum EffectSWFlags
        {
            None = 0,
            EnvEffectArea00 = 1,
            EnvEffectArea01 = 1 << 1,
            EnvEffectArea02 = 1 << 2,
            EnvEffectArea03 = 1 << 3,
            EnvEffectArea04 = 1 << 4,
            EnvEffectArea05 = 1 << 5,
            EnvEffectArea06 = 1 << 6,
            EnvEffectArea07 = 1 << 7,
            EnvEffectArea08 = 1 << 8,
            EnvEffectArea09 = 1 << 9,
            EnvEffectArea10 = 1 << 10,
            EnvEffectArea11 = 1 << 11,
            EnvEffectArea12 = 1 << 12,
            EnvEffectArea13 = 1 << 13,
            EnvEffectArea14 = 1 << 14,
            EnvEffectArea15 = 1 << 15,
        }

        public EffectArea()
        {
            EffectSW = EffectSWFlags.None;
            Scale = new ByamlVector3F(1, 1, 1);
        }

        public object Clone()
        {
            return new EffectArea()
            {
                EffectSW = this.EffectSW,
                Prm1 = this.Prm1,
                Prm2 = this.Prm2,
                Rotate = this.Rotate,
                Scale = this.Scale,
                Translate = this.Translate,
            };
        }
    }
}
