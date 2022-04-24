using System.Collections.Generic;
using System.Diagnostics;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an object in the course which can be referenced by its <see cref="UnitIdNum"/>.
    /// </summary>
    [ByamlObject]
    [DebuggerDisplay("{GetType().Name}  UnitIdNum={UnitIdNum}")]
    public abstract class UnitObject
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a number identifying this object. Can be non-unique or 0 without any issues.
        /// </summary>
        [ByamlMember]
        public int UnitIdNum { get; set; }
    }
}
