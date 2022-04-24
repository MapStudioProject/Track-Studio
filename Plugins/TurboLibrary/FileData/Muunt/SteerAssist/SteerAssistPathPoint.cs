using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of an <see cref="SteerAssistPath"/>.
    /// </summary>
    [ByamlObject]
    public class SteerAssistPathPoint : PathPointBase<SteerAssistPath, SteerAssistPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an unknown priority.
        /// </summary>
        [ByamlMember]
        [BindGUI("Priority", Category = "Properties")]
        public SteerPriority Priority { get; set; } = SteerPriority.Default;

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<SteerAssistPath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.SteerAssistPaths;
        }

        public enum SteerPriority
        {
            Safety_Fallback = 0,
            Default = 1,
            Shortcut = 2,
            BulletBill_BranchOff = 3,
            Shortcut_Speed_Required = 9,
        }
    }
}
