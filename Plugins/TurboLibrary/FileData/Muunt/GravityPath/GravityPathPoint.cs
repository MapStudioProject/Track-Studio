using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of a <see cref="GravityPath"/>.
    /// </summary>
    [ByamlObject]
    public class GravityPathPoint : PathPointBase<GravityPath, GravityPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the pre determined height preset for how camera is positioned.
        /// The game uses a range of 0 - 2 values.
        /// </summary>
        [ByamlMember]
        [BindGUI("Camera Height", Category = "Properties")]
        public CameraHeightType CameraHeight { get; set; }

        /// <summary>
        /// Gets or sets a value possibly indicating whether this gravity path is only effective when gliding.
        /// </summary>
        [ByamlMember]
        [BindGUI("Usable With Glide Only", Category = "Properties")]
        public bool GlideOnly { get; set; }

        /// <summary>
        /// Gets or sets a value that determines to activate the anti g transformation of the kart.
        /// </summary>
        [ByamlMember]
        [BindGUI("Transform", Category = "Properties")]
        public bool Transform { get; set; }

        public enum CameraHeightType : int
        {
            NoChange = 0,
            Higher = 1,
            Highest = 2,
        }

        public GravityPathPoint()
        {
            Transform = true;
        }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<GravityPath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.GravityPaths;
        }
    }
}
