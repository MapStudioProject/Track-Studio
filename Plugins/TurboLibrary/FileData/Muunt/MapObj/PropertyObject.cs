using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary
{
    public class PropertyObject
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public PropertyObject(string name, object value) {
            Name = name;
            Value = value;
        }
    }
}
