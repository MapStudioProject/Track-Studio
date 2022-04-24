using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of an <see cref="ItemPath"/>.
    /// </summary>
    [ByamlObject]
    public class ItemPathPoint : PathPointBase<ItemPath, ItemPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value possibly indicating whether the item is allowed to move above the original path.
        /// </summary>
        [ByamlMember]
        [BindGUI("Hover", Category = "Properties")]
        public HoverType Hover { get; set; }

        /// <summary>
        /// Gets or sets a value indicating an unknown search area.
        /// </summary>
        [ByamlMember]
        [BindGUI("Search Area", Category = "Properties")]
        public SearchType SearchArea { get; set; }

        /// <summary>
        /// Gets or sets an unknown priority.
        /// </summary>
        [ByamlMember]
        [BindGUI("Item Priority", Category = "Properties")]
        public PriorityType ItemPriority { get; set; } = PriorityType.Default;

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<ItemPath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.ItemPaths;
        }

        public enum HoverType
        {
            None,
            HasHover, //red + blue shells can hover, no gravity
            Cannon, //blue shells only
        }

        public enum SearchType
        {
            SmallSearch = 0,
            BigSearch = 1,
        }

        public enum PriorityType
        {
            Safety_Fallback = 0,
            Default = 1,
            Shortcut = 2,
            Unknown = 3,
        }
    }
}
