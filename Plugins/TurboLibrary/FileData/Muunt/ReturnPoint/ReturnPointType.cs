namespace TurboLibrary
{
    /// <summary>
    /// Gets or sets values seen in hasError members.
    /// </summary>
    public enum ReturnPointType
    {
        /// <summary>
        /// Default value
        /// </summary>
        None = -1,

        /// <summary>
        /// Ignores and goes to nearest point (that isn't type 1)
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Does not return to this point after pass (typically used on ramps)
        /// </summary>
        NoReturnAfterPass = 1,
    }
}
