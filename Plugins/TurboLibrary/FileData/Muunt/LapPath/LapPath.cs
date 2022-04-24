using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a path which drivers need to taken to complete a lap.
    /// </summary>
    [ByamlObject]
    public class LapPath : PathBase<LapPath, LapPathPoint>
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        [ByamlMember("LapPath_GravityPath", Optional = true)]
        private List<int> _gravityPathIndices;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating the group the lap path belongs to, possibly for multiple routes.
        /// </summary>
        [ByamlMember("LapPathGroup")]
        [BindGUI("Group")]
        public int Group { get; set; }

        /// <summary>
        /// Gets or sets an unknown value, possibly handling Lakitu return locations and referencing
        /// <see cref="ReturnPoint"/> instances.
        /// </summary>
        [ByamlMember]
        [BindGUI("Return Points Error")]
        public bool ReturnPointsError { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ReturnPoint"/> instances.
        /// </summary>
        [ByamlMember(Optional = true)]
        public List<ReturnPoint> ReturnPoints { get; set; }

        /// <summary>
        /// Gets or sets a list of <see cref="GravityPaths"/> instances used by this lap path.
        /// </summary>
        public List<GravityPath> GravityPaths { get; set; }

        public LapPath()
        {
            ReturnPoints = new List<ReturnPoint>();
            ReturnPointsError = false;
            Group = -1;
            GravityPaths = null;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public override void DeserializeReferences(CourseDefinition courseDefinition)
        {
            base.DeserializeReferences(courseDefinition);

            if (ReturnPoints != null) {
                foreach (var pt in ReturnPoints) {
                    pt.DeserializeReferences(courseDefinition);
                }
                for (int i = 0; i < Points.Count; i++)
                {
                    if (ReturnPoints.Count > i)
                        Points[i].ReturnPoint = ReturnPoints[i];
                    else
                        Points[i].ReturnPoint = new ReturnPoint() { ReturnType = ReturnPointType.Ignore };
                }
            }

            // Gravity paths.
            if (_gravityPathIndices == null)
            {
                GravityPaths = null;
            }
            else
            {
                GravityPaths = new List<GravityPath>();
                foreach (int index in _gravityPathIndices)
                {       
                    GravityPaths.Add(courseDefinition.GravityPaths[index]);
                }
            }
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public override void SerializeReferences(CourseDefinition courseDefinition)
        {
            base.SerializeReferences(courseDefinition);

            ReturnPoints = new List<ReturnPoint>();
            for (int i = 0; i < Points.Count; i++)
                ReturnPoints.Add(Points[i].ReturnPoint);

            if (ReturnPoints != null) {
                foreach (var pt in ReturnPoints) {
                    pt.SerializeReferences(courseDefinition);
                }
            }

            // Gravity paths.
            if (GravityPaths == null)
            {
                _gravityPathIndices = null;
            }
            else
            {
                _gravityPathIndices = new List<int>();
                foreach (GravityPath path in GravityPaths)
                {
                    int index = courseDefinition.GravityPaths.IndexOf(path);
                    if (index != -1)
                        _gravityPathIndices.Add(index);
                }
            }
        }

        public override string ToString()
        {
            return $"Group #{Group} ReturnPointsError {ReturnPointsError}";
        }
    }
}
