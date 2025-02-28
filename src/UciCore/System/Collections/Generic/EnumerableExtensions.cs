using System;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic;

internal readonly record struct IndexedItem<T>(int Index, T Item, bool IsFirst, bool IsLast);

internal static class EnumerableExtensions
{
    public static IEnumerable<IndexedItem<T>> IndexEx<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is null)
        {
            throw new ArgumentNullException(nameof(enumerable));
        }

        // we separate the null check from the method body with yield, otherwise the null check will not be executed until start enumerating.
        // see details in https://stackoverflow.com/questions/42149895/method-having-yield-return-is-not-throwing-exception
        return IndexExIterator(enumerable);

        static IEnumerable<IndexedItem<T>> IndexExIterator(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var i = 0;
            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return new(i, current, i == 0, false);
                current = enumerator.Current;
                ++i;
            }

            yield return new(i, current, i == 0, true);
        }
    }
}
