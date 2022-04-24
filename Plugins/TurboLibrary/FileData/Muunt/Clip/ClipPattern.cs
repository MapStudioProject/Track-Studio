using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents the clip pattern node in a course definition file.
    /// </summary>
    [ByamlObject]
    public class ClipPattern
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the unknown &quot;StartOnly&quot; value.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("Start Only", Category = "Properties")]
        public int? StartOnly { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AreaFlag"/> instance.
        /// </summary>
        [ByamlMember]
        public List<AreaFlag> AreaFlag { get; set; }

        public ClipPattern()
        {
            AreaFlag = new List<AreaFlag>();
        }
    }
}
