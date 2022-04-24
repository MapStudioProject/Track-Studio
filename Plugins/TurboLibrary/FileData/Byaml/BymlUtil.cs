using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary
{
    public static class BymlUtil
    {
        public static int GetInt(dynamic value, int defaultValue = -1)
        {
            if (value == null)
                return defaultValue;
            else
                return value;
        }

        public static bool GetBool(dynamic value, bool defaultValue = false)
        {
            if (value == null)
                return defaultValue;
            else
                return value;
        }

        public static float GetFloat(dynamic value, float defaultValue = 0)
        {
            if (value == null)
                return defaultValue;
            else
                return value;
        }
    }
}
