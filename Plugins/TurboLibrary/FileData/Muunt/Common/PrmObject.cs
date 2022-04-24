using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an object on a course which is translated, rotated and scaled in space.
    /// </summary>
    [ByamlObject]
    public abstract class PrmObject : SpatialObject
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the first parameter.
        /// </summary>
        [ByamlMember("prm1")]
        [BindGUI("Param 1", Category = "Params", ColumnIndex = 0)]
        public float Prm1 { get; set; }

        /// <summary>
        /// Gets or sets the second parameter.
        /// </summary>
        [ByamlMember("prm2")]
        [BindGUI("Param 2", Category = "Params", ColumnIndex = 1)]
        public float Prm2 { get; set; }
    }
}
