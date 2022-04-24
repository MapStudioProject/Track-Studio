using System.Collections.Generic;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of an <see cref="ObjPath"/>.
    /// </summary>
    [ByamlObject]
    public class ObjPathPoint : PathPointBase<ObjPath, ObjPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an index into the points of the parent's BYAML path.
        /// </summary>
        [ByamlMember("Index")]
        public int PathIndex { get; set; }

        /// <summary>
        /// Gets or sets the first parameter.
        /// </summary>
        [ByamlMember("prm1")]
        public float Prm1 { get; set; }

        /// <summary>
        /// Gets or sets the second parameter.
        /// </summary>
        [ByamlMember("prm2")]
        public float Prm2 { get; set; }

        /// <summary>
        /// Gets or sets the list of tangential smoothing points (must be exactly 2).
        /// </summary>
        [ByamlMember("TS_BakedControlPoints", Optional = true)]
        public List<ByamlVector3F> ControlPoints { get; set; }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<ObjPath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.ObjPaths;
        }
    }
}
