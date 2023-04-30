using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Helveg;

/// <summary>
/// A bunch of polyfills and methods that I consider to be missing from .NET.
/// </summary>
internal static class CollectionExtensions
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

    public static int FindIndex<T>(this ImmutableArray<T> array, int startIndex, int count, Predicate<T> predicate)
    {
        if (array.IsDefaultOrEmpty)
        {
            return -1;
        }

        var end = Math.Min(startIndex + count, array.Length);
        for (int i = startIndex; i < end; i++)
        {
            if (predicate(array[i]))
            {
                return i;
            }
        }
        return -1;
    }

    public static int FindIndex<T>(this ImmutableArray<T> array, int startIndex, Predicate<T> predicate)
    {
        if (array.IsDefaultOrEmpty)
        {
            return -1;
        }

        return array.FindIndex(startIndex, array.Length, predicate);
    }

    public static int FindIndex<T>(this ImmutableArray<T> array, Predicate<T> predicate)
    {
        if (array.IsDefaultOrEmpty)
        {
            return -1;
        }

        return array.FindIndex(0, array.Length, predicate);
    }
}
