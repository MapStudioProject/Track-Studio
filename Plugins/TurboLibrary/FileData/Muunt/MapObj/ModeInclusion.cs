using System;
using System.Collections.Generic;

namespace TurboLibrary
{
    /// <summary>
    /// Represents the possible game modes in which a unit object can appear.
    /// </summary>
    [Flags]
    public enum ModeInclusion
    {
        /// <summary>
        /// No mode.
        /// </summary>
        None = 0,

        /// <summary>
        /// Multiplayer mode with 2 players (vertical splitscreen).
        /// </summary>
        Multi2P = 1 << 0,

        /// <summary>
        /// Multiplayer mode with 3 or 4 players (quad splitscreen).
        /// </summary>
        Multi4P = 1 << 1,

        /// <summary>
        /// Online 1-player mode.
        /// </summary>
        WiFi = 1 << 2,

        /// <summary>
        /// Online 2-player mode (vertical splitscreen).
        /// </summary>
        WiFi2P = 1 << 3
    }

    /// <summary>
    /// Represents a set of extension methods for the <see cref="ModeInclusion"/> enumeration.
    /// </summary>
    public static class ModeInclusionExtensions
    {
        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a <see cref="ModeInclusion"/> value according to the Multi2P, Multi4P, WiFi2P and WiFi4P entries of the
        /// given <paramref name="dictionary"/>.
        /// </summary>
        /// <param name="modeInclusion">The extended <see cref="ModeInclusion"/> instance.</param>
        /// <param name="dictionary">The <see cref="IDictionary{String, Object}"/> to retrieve the entries from.</param>
        /// <returns>A <see cref="ModeInclusion"/> value according to the dictionary.</returns>
        public static ModeInclusion FromDictionary(this ModeInclusion modeInclusion,
            IDictionary<string, object> dictionary)
        {
            modeInclusion = ModeInclusion.None;

            // Note that the BYAML values are negated, they are true if an object is excluded from track.
            if (!(bool)dictionary["Multi2P"]) modeInclusion |= ModeInclusion.Multi2P;
            if (!(bool)dictionary["Multi4P"]) modeInclusion |= ModeInclusion.Multi4P;
            if (!(bool)dictionary["WiFi"]) modeInclusion |= ModeInclusion.WiFi;
            if (!(bool)dictionary["WiFi2P"]) modeInclusion |= ModeInclusion.WiFi2P;
            return modeInclusion;
        }

        /// <summary>
        /// Sets the entries of the given <paramref name="dictionary"/> according to the current instance.
        /// </summary>
        /// <param name="modeInclusion">The extended <see cref="ModeInclusion"/> instance.</param>
        /// <param name="dictionary">The <see cref="IDictionary{String, Object}"/> to configure.</param>
        public static void ToDictionary(this ModeInclusion modeInclusion, IDictionary<string, object> dictionary)
        {
            // Note that the BYAML values are negated, they are true if an object is excluded from track.
            dictionary["Multi2P"] = !modeInclusion.HasFlag(ModeInclusion.Multi2P);
            dictionary["Multi4P"] = !modeInclusion.HasFlag(ModeInclusion.Multi4P);
            dictionary["WiFi"] = !modeInclusion.HasFlag(ModeInclusion.WiFi);
            dictionary["WiFi2P"] = !modeInclusion.HasFlag(ModeInclusion.WiFi2P);
        }
    }
}
