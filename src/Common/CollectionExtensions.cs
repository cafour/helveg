using System;
using System.Collections.Generic;

namespace Helveg;

/// <summary>
/// A polyfill for System.Collections.Generic.CollectionExtensions methods which are not available in netstandard2.0.
/// </summary>
public static class CollectionExtensions
{
    public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
       => dictionary.GetValueOrDefault(key, default!);

    public static TValue GetValueOrDefault<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue)
    {
        if (dictionary is null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        return dictionary.TryGetValue(key, out TValue? value) ? value : defaultValue;
    }
}
