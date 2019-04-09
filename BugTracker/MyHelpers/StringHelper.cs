using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.MyHelpers
{
    public static class StringHelper
    {
        public static T ParseEnum<T>(this string value) => (T)Enum.Parse(typeof(T), value, true);
    }
}