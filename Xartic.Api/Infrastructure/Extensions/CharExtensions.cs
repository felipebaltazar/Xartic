namespace Xartic.Api.Infrastructure.Extensions
{
    public static class CharExtensions
    {
        public static bool EqualsIgnoreCase(this char source, char toCompare) =>
            char.ToUpperInvariant(source) == char.ToUpperInvariant(toCompare);
    }
}
