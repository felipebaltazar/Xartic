namespace Martic.Abstractions
{
    public interface ILazyDependency<T>
    {
        T Value { get; }
    }
}
