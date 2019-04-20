using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.MyHelpers
{
    [NotMapped]
    public static class StringHelper
    {
        public static string GuidPlusSuffix(this Guid guid, string suffix) => guid.ToString("N").ToUpper() + suffix;
        public static T ParseEnum<T>(this string value) => (T)Enum.Parse(typeof(T), value, true);
    }
}