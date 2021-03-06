namespace System.Linq
{
    using System.Collections.Generic;

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TSource> ElementsIn<TSource>(this IEnumerable<TSource> source, Range range)
        {
            if (source == null)
            {
                // ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                throw new ArgumentNullException(nameof(source));
            }

            return ElementsInIterator(source, range);
        }

        private static IEnumerable<TSource> ElementsInIterator<TSource>(IEnumerable<TSource> source, Range range)
        {
            Index start = range.Start;
            Index end = range.End;

            if (source is IList<TSource> list)
            {
                int count = list.Count;
                if (count == 0 && range.Equals(System.Range.All))
                {
                    yield break;
                }

                int firstIndex = start.IsFromEnd ? count - start.Value : start.Value;
                int lastIndex = (end.IsFromEnd ? count - end.Value : end.Value) - 1;
                if (lastIndex < firstIndex - 1 || firstIndex < 0 || lastIndex < 0 || firstIndex >= count || lastIndex >= count)
                {
                    // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                    throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                }

                for (int currentIndex = firstIndex; currentIndex <= lastIndex; currentIndex++)
                {
                    yield return list[currentIndex];
                }
                yield break;
            }

            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                int currentIndex = -1;
                if (start.IsFromEnd)
                {
                    if (!e.MoveNext())
                    {
                        // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                        throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                    }

                    Queue<TSource> queue = new Queue<TSource>();
                    queue.Enqueue(e.Current);
                    currentIndex++;

                    int takeLastCount = start.Value;
                    while (e.MoveNext())
                    {
                        if (queue.Count == takeLastCount)
                        {
                            queue.Dequeue();
                        }

                        queue.Enqueue(e.Current);
                        currentIndex++;
                    }

                    if (queue.Count < takeLastCount)
                    {
                        // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                        throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                    }

                    int firstIndex = currentIndex + 1 - takeLastCount;
                    int lastIndex = end.IsFromEnd ? currentIndex - end.Value : end.Value - 1;
                    if (lastIndex < firstIndex - 1)
                    {
                        // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                        throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                    }

                    for (int index = firstIndex; index <= lastIndex; index++)
                    {
                        yield return queue.Dequeue();
                    }
                }
                else
                {
                    int firstIndex = start.Value;
                    if (!e.MoveNext())
                    {
                        if (range.Equals(System.Range.All))
                        {
                            yield break;
                        }

                        // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                        throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                    }

                    currentIndex++;
                    while (currentIndex < firstIndex && e.MoveNext())
                    {
                        currentIndex++;
                    }

                    if (currentIndex != firstIndex)
                    {
                        // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                        throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                    }

                    if (end.IsFromEnd)
                    {
                        int skipLastCount = end.Value;
                        if (skipLastCount > 0)
                        {
                            Queue<TSource> queue = new Queue<TSource>();
                            do
                            {
                                if (queue.Count == skipLastCount)
                                {
                                    yield return queue.Dequeue();
                                }

                                queue.Enqueue(e.Current);
                                currentIndex++;
                            }
                            while (e.MoveNext());
                        }
                        else
                        {
                            do
                            {
                                yield return e.Current;
                                currentIndex++;
                            }
                            while (e.MoveNext());
                        }

                        if (firstIndex + skipLastCount > currentIndex)
                        {
                            // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                            throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                        }
                    }
                    else
                    {
                        int lastIndex = end.Value - 1;
                        if (lastIndex < firstIndex - 1)
                        {
                            // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                            throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                        }

                        if (lastIndex == firstIndex - 1)
                        {
                            yield break;
                        }

                        yield return e.Current;
                        while (currentIndex < lastIndex && e.MoveNext())
                        {
                            currentIndex++;
                            yield return e.Current;
                        }

                        if (currentIndex != lastIndex)
                        {
                            // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.range);
                            throw new ArgumentOutOfRangeException(nameof(range)); // Following the behavior of array with range.
                        }
                    }
                }
            }
        }
    }
}