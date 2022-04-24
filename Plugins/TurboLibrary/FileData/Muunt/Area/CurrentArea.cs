using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary
{
    public class CurrentArea : Area
    {
        public CurrentArea() : base()
        {
            AreaType = AreaType.Current_DeluxeOnly;
        }
    }
}
