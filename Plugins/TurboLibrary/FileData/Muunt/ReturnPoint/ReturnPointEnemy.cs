using System;
using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a return point of an <see cref="EnemyPath"/>.
    /// </summary>
    [ByamlObject]
    public class ReturnPointEnemy : IByamlSerializable, ICourseReferencable
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an unknown return type.
        /// </summary>
        [ByamlMember]
        [BindGUI("Return Type", Category = "Properties")]
        public int ReturnType { get; set; } = -1;

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [BindGUI("Has Error", Category = "Properties")]
        public ReturnPointErrorType HasError { get; set; }
/*
        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember("JugemIndex")]
        [BindGUI("Jugem Index", Category = "Properties")]
        public int JugemIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember("JugemPath")]
        [BindGUI("Jugem Path", Category = "Properties")]
        public string JugemPath { get; set; } = ""; 

        */

        ///// <summary>
        ///// Gets or sets a referenced <see cref="JugemPathPoint"/>.
        ///// </summary>
        //public JugemPathPoint JugemPathPoint { get; set; }

        /// <summary>
        /// Gets or sets the spatial normal.
        /// </summary>
        [ByamlMember]
        [BindGUI("Normal", Category = "Properties")]
        public ByamlVector3F Normal { get; set; }

        /// <summary>
        /// Gets or sets the spatial position.
        /// </summary>
        [ByamlMember]
        [BindGUI("Position", Category = "Properties")]
        public ByamlVector3F Position { get; set; }

        /// <summary>
        /// Gets or sets the spatial tangent.
        /// </summary>
        [ByamlMember]
        [BindGUI("Tangent", Category = "Properties")]
        public ByamlVector3F Tangent { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Reads data from the given <paramref name="dictionary"/> to satisfy members.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{String, Object}"/> to read the data from.</param>
        public void DeserializeByaml(IDictionary<string, object> dictionary)
        {
            HasError = (ReturnPointErrorType)Enum.Parse(typeof(ReturnPointErrorType), (string)dictionary["hasError"]);
        }

        /// <summary>
        /// Writes instance members into the given <paramref name="dictionary"/> to store them as BYAML data.
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{String, Object}"/> to store the data in.</param>
        public void SerializeByaml(IDictionary<string, object> dictionary)
        {
            dictionary["hasError"] = HasError.ToString();
        }

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            // TODO: Since JugemPath is just a string here, no clue what is meant with the path and point members.
            //JugemPath jugemPath = courseDefinition.JugemPaths[_jugemPathIndex];
            //JugemPathPoint = jugemPath.Points[_jugemPathPointIndex];
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            // TODO: Since JugemPath is just a string here, no clue what is meant with the path and point members.
            //_jugemPathIndex = courseDefinition.JugemPaths.IndexOf(JugemPathPoint.Path);
            //_jugemPathPointIndex = JugemPathPoint.Index;
        }
    }
}
