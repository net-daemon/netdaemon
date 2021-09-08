using System;
using System.Collections.Generic;
using System.Linq;
namespace NetDaemon.Service.App.CodeGeneration.Extensions
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<TSource> Duplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var grouped = source.GroupBy(selector);
            var moreThan1 = grouped.Where(i => i.IsMultiple());
            return moreThan1.SelectMany(i => i);
        }

        public static IEnumerable<TSource> HandleDuplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, Func<TSource, TSource> func)
        {
            var sourceList = source.ToList();

            Duplicates(sourceList, selector).ToList().ForEach(x =>
            {
                sourceList.Remove(x);

                x = func(x);

                sourceList.Add(x);
            });

            return sourceList;
        }

        public static bool IsMultiple<T>(this IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();
            return enumerator.MoveNext() && enumerator.MoveNext();
        }
    }
}