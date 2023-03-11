namespace Helveg.Abstractions;

/// <summary>
/// Used for types that guaratee that their default instance is Invalid.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IInvalidable<T>
    where T : IInvalidable<T>
{
    public abstract static T Invalid { get; }
}
