using System.Collections.Generic;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a path controlling the gravity direction of the course.
    /// </summary>
    [ByamlObject]
    public class GravityPath : PathBase<GravityPath, GravityPathPoint>
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        [ByamlMember("GravityPath_GCameraPath", Optional = true)]
        private List<int> _gCameraPathIndices;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the list of <see cref="GCameraPath"/> instances this gravity path uses.
        /// </summary>
        public List<GCameraPath> GCameraPaths { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public override void DeserializeReferences(CourseDefinition courseDefinition)
        {
            // Solve the point references.
            base.DeserializeReferences(courseDefinition);

            // Solve the GCamera path references.
            if (_gCameraPathIndices != null)
            {
                GCameraPaths = new List<GCameraPath>();
                foreach (int index in _gCameraPathIndices)
                {
                    GCameraPaths.Add(courseDefinition.GCameraPaths[index]);
                }
            }
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public override void SerializeReferences(CourseDefinition courseDefinition)
        {
            // Solve the point references.
            base.SerializeReferences(courseDefinition);

            // Solve the GCamera path references.
            if (GCameraPaths != null)
            {
                _gCameraPathIndices = new List<int>();
                foreach (GCameraPath path in GCameraPaths)
                {
                    int index = courseDefinition.GCameraPaths.IndexOf(path);
                    if (index != -1)
                        _gCameraPathIndices.Add(courseDefinition.GCameraPaths.IndexOf(path));
                }
            }
        }
    }
}
