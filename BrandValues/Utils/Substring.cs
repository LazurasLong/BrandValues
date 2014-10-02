using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrandValues.Utils
{
    public static class Substring
    {
        public static string TruncateLongString(this string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }
    }
}