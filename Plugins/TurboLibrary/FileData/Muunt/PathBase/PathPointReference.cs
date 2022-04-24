
namespace TurboLibrary
{
    /// <summary>
    /// Represents the serialized index to a path and a point in it from the course definition.
    /// </summary>
    [ByamlObject]
    public class PathPointReference
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PathPointReference"/> class.
        /// </summary>
        public PathPointReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathPointReference"/> class with the given indices.
        /// </summary>
        /// <param name="pathIndex">The path index.</param>
        /// <param name="pointIndex">The point index.</param>
        internal PathPointReference(int pathIndex, int pointIndex)
        {
            PathIndex = pathIndex;
            PointIndex = pointIndex;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the index of the path instance referenced from the list of enemy paths in the course
        /// definition.
        /// </summary>
        [ByamlMember("PathId")]
        internal int PathIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the path point of the path referenced by <see cref="PathIndex"/>.
        /// </summary>
        [ByamlMember("PtId")]
        internal int PointIndex { get; set; }
    }
}
