using System.Collections.Generic;
using ByamlExt.Byaml;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a path possibly determining where Lakitu resets drivers back to.
    /// </summary>
    [ByamlObject]
    public class JugemPath : PathBase<JugemPath, JugemPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember]
        public int PtNum { get; set; }

        /// <summary>
        /// Gets or sets a value possibly indicating the thickness of the path.
        /// </summary>
        [ByamlMember]
        [BindGUI("Split Width", Category = "Properties")]
        public float SplitWidth { get; set; }

        /// <summary>
        /// Gets a BYAML path embedded in this path for unknown reasons.
        /// </summary>
        [ByamlMember]
        public List<ByamlPathPoint> ObjPt { get; set; }
    }
}
