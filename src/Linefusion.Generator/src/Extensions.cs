using System.Collections.Generic;
using System.Linq;

namespace Linefusion.Generator;

public static class ArrayExtensions
{
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize = 1)
    {
        while (source.Any())
        {
            yield return source.Take(chunkSize);
            source = source.Skip(chunkSize);
        }
    }
}