using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of an <see cref="KillerPath"/>.
    /// </summary>
    [ByamlObject]
    public class KillerPathPoint : PathPointBase<KillerPath, KillerPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<KillerPath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.KillerPaths;
        }
    }
}
