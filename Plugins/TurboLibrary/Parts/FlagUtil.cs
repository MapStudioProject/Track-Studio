using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsEditor
{
    public class FlagUtil
    {
        public static void SetFlag(ref int flags, int mask, bool value)
        {
            if (value)
            {
                flags |= mask;
            }
            else
            {
                flags &= ~mask;
            }
        }
    }
}
