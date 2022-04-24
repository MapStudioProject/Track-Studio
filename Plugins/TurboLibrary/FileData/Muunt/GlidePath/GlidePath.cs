using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a path a gliding driver is flying along.
    /// </summary>
    [ByamlObject]
    public class GlidePath : PathBase<GlidePath, GlidePathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating the type of gliding.
        /// </summary>
        [ByamlMember]
        [BindGUI("Glide Type", Category = "Properties")]
        public GliderType GlideType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is an updraft glide.
        /// </summary>
        [ByamlMember]
        [BindGUI("Use Updraft", Category = "Properties")]
        public bool IsUp { get; set; }

        public enum GliderType : int
        {
            Normal,
            Cannon,
        }
    }
}
