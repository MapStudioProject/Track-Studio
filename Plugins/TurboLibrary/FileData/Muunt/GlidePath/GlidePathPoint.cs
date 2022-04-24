using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of a <see cref="GlidePath"/>.
    /// </summary>
    [ByamlObject]
    public class GlidePathPoint : PathPointBase<GlidePath, GlidePathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        [BindGUI("Path", Category = "Path")]
        public GlidePath PathProperties => this.Path;

        /// <summary>
        /// Gets or sets a value indicating whether the driver is pulled as if shot through a cannon.
        /// </summary>
        [ByamlMember]
        [BindGUI("Is Cannon", Category = "Properties")]
        public bool Cannon { get; set; }

        [ByamlMember(Optional = true)]
        [BindGUI("Ascend", Category = "Properties")]
        public int? Ascend { get; set; }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<GlidePath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.GlidePaths;
        }
    }
}
