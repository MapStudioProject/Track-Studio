using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of an <see cref="EnemyPath"/>.
    /// </summary>
    [ByamlObject]
    public class EnemyPathPoint : PathPointBase<EnemyPath, EnemyPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an unknown battle flag.
        /// </summary>
        [ByamlMember]
        [BindGUI("Battle Flags", Category = "Properties")]
        public int BattleFlag { get; set; }

        /// <summary>
        /// Gets or sets an unknown point direction.
        /// </summary>
        [ByamlMember]
        [BindGUI("Path Direction", Category = "Properties")]
        public AIPathDir PathDir { get; set; }

        /// <summary>
        /// Gets or sets an unknown point priority.
        /// </summary>
        [ByamlMember]
        [BindGUI("Priority", Category = "Properties")]
        public AIPriority Priority { get; set; } = AIPriority.Default;

        public EnemyPathPoint() : base()
        {
            this.Scale = new ByamlVector3F(250, 250, 250);
        }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<EnemyPath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.EnemyPaths;
        }

        public enum AIPathDir
        {
            None = 0,
            NextPointAfter,
            UnknownDirection,
        }

        public enum AIPriority
        {
            Safety_Fallback = 0,
            Default = 1,
            Shortcut = 2,
            BulletBill_BranchOff = 3,
            Shortcut_Speed_Required = 9,
        }
    }
}
