namespace TurboLibrary
{
    /// <summary>
    /// Represents the action taken for objects inside of an area or clip area.
    /// </summary>
    public enum AreaType
    {
        /// <summary>
        /// Areas that link to cameras.
        /// </summary>
        Camera = 0,

        /// <summary>
        /// Unknown area type. Appears in Mario Circuit and Twisted Mansion.
        /// </summary>
        Unknown1 = 1,

        /// <summary>
        /// Audio area type.
        /// </summary>
        Audio = 2,

        /// <summary>
        /// Objects are moved along a specified path.
        /// </summary>
        Pull = 3,

        /// <summary>
        /// Objects will roam randomly inside of this area.
        /// </summary>
        Roam = 4,

        /// <summary>
        /// Specifies that this is a clip area. Not valid for areas.
        /// </summary>
        Clip = 5,

        Current_DeluxeOnly = 6,

        Prison_DeluxeOnly = 7,

        Ceiling_DeluxeOnly = 8,
    }
}
