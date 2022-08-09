using System.Globalization;

namespace Martic.Infrastructure.Extensions
{
    public static class DoubleExtensions
    {
        public static float ToSingle(this double value) =>
            Convert.ToSingle(value, CultureInfo.InvariantCulture);
    }
}
