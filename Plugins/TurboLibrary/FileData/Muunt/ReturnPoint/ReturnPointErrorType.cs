namespace TurboLibrary
{
    /// <summary>
    /// Gets or sets values seen in hasError members.
    /// </summary>
    public enum ReturnPointErrorType
    {
        /// <summary>
        /// Unknown value.
        /// </summary>
        None = 0,

        /// <summary>
        /// </summary>
        NoFallError = 2,

        /// <summary>
        /// Unknown value. Name is a guess on the ReturnPointEnemy names seen for hasError as it is the only other
        /// appearing integer for ReturnPoint.
        /// </summary>
        NoCollision = 3,
    }
}
