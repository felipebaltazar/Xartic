using System;
using System.Globalization;

namespace Xartic.App.Extensions
{
    public static class DoubleExtensions
    {
        public static float ToSingle(this double value)
        {
            return Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }
    }
}
