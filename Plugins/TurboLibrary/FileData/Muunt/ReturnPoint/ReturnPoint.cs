using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a return point of a <see cref="LapPath"/>.
    /// </summary>
    [ByamlObject]
    public class ReturnPoint : ICourseReferencable
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        [ByamlMember("JugemPath")]
        private int _jugemPathIndex; // TODO: JugemPath member in ReturnPointEnemy is a string with "rail2" etc.
        [ByamlMember("JugemIndex")]
        private int _jugemPathPointIndex;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an unknown return type.
        /// </summary>
        [ByamlMember]
        [BindGUI("Return Type", Category = "Return Point")]
        public ReturnPointType ReturnType { get; set; } = ReturnPointType.None;

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [BindGUI("Has Error", Category = "Return Point")]
        [ByamlMember("hasError")]
        public ReturnPointErrorType HasError { get; set; } = ReturnPointErrorType.None;

        /// <summary>
        /// Gets or sets a referenced <see cref="JugemPath"/>.
        /// </summary>
        public JugemPath JugemPath { get; set; }

        /// <summary>
        /// Gets or sets a referenced <see cref="JugemPathPoint"/>.
        /// </summary>
        public ByamlExt.Byaml.ByamlPathPoint JugemPathPoint { get; set; }

        /// <summary>
        /// Gets or sets the spatial position.
        /// </summary>
        [ByamlMember]
        public ByamlVector3F Position { get; set; }

        /// <summary>
        /// Gets or sets the spatial normal.
        /// </summary>
        [ByamlMember]
        public ByamlVector3F Normal { get; set; }

        /// <summary>
        /// Gets or sets the spatial tangent.
        /// </summary>
        [ByamlMember]
        public ByamlVector3F Tangent { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            if (courseDefinition.JugemPaths != null && _jugemPathIndex < courseDefinition.JugemPaths.Count) {
                if (_jugemPathIndex != -1 && courseDefinition.JugemPaths.Count > _jugemPathIndex)
                    JugemPath = courseDefinition.JugemPaths[_jugemPathIndex];
                if (JugemPath != null && JugemPath.ObjPt.Count > _jugemPathPointIndex && _jugemPathPointIndex != -1)
                JugemPathPoint = JugemPath != null ? JugemPath.ObjPt[_jugemPathPointIndex] : null;
            }
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            if (courseDefinition.JugemPaths != null && JugemPath != null)
                _jugemPathIndex = courseDefinition.JugemPaths.IndexOf(JugemPath);
            else
                _jugemPathIndex = -1;

            if (JugemPath != null && courseDefinition.JugemPaths.Contains(JugemPath))
                _jugemPathPointIndex = JugemPath.ObjPt.IndexOf(JugemPathPoint);
            else
                _jugemPathPointIndex = -1;
        }

        public override string ToString() => "Return Point";
    }
}
