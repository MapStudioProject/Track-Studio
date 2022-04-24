using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class EnvObject
    {
        internal virtual ParamObject Parent { get; set; }

        /// <summary>
        /// Gets or sets a value that toggles this point light object's visibilty.
        /// </summary>
        [BindGUI("Enable", Category = "Properties", Order = 0)]
        public bool Enable
        {
            get { return Parent.GetEntryValue<bool>("enable"); }
            set { Parent.SetEntryValue("enable", value); }
        }

        /// <summary>
        /// Gets or sets the name of this object.
        /// </summary>
        [BindGUI("Name", Category = "Properties", Order = 1)]
        public string Name
        {
            get { return Parent.GetEntryValue<StringEntry>("name").ToString(); }
            set { Parent.SetEntryValue("name", new StringEntry(value, 32)); }
        }

        /// <summary>
        /// Gets or sets the group of this object.
        /// </summary>
        [BindGUI("Group", Category = "Properties", Order = 2)]
        public string Group
        {
            get { return Parent.GetEntryValue<StringEntry>("group").ToString(); }
            set { Parent.SetEntryValue("group", new StringEntry(value, 32)); }
        }

    }
}
