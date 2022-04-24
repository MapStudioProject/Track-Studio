namespace TurboLibrary
{
    /// <summary>
    /// Represents a BYAML element which references others and thus must resolve and build the dependencies.
    /// </summary>
    public interface ICourseReferencable
    {
        // ---- METHODS ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        void DeserializeReferences(CourseDefinition courseDefinition);

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        void SerializeReferences(CourseDefinition courseDefinition);
    }
}
