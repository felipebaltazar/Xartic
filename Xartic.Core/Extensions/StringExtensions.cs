using System;
using System.Linq;

namespace Xartic.Core.Extensions
{
    public static class StringExtensions
    {
        public static string GetResource(this string[] resources, string name)
        {
            return resources?
                .FirstOrDefault(r => r.EndsWithIgnoreCase(name));
        }

        public static bool EndsWithIgnoreCase(this string source, string strToCompare)
        {
            if (source == strToCompare)
                return true;

            return (source ?? string.Empty)
                .EndsWith(strToCompare ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}
