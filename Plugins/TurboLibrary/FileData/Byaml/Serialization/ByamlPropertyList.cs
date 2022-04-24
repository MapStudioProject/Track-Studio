using System;

namespace TurboLibrary
{
    /// <summary>
    /// Stores a list of all the properties in the current byml node.
    /// The object for this should be a Dictionary of dynamic values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ByamlPropertyList : Attribute
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public ByamlPropertyList()
        {

        }
    }
}
