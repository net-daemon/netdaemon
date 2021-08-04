using System;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Daemon
{
    internal static class AppSorter
    {
        internal static IReadOnlyList<INetDaemonApp> SortByDependency(IReadOnlyList<INetDaemonApp> unsortedList)
        {
            var dependencies = unsortedList.SelectMany(n => n.Dependencies).ToHashSet();

            if (dependencies.Count == 0) return unsortedList;

            // just make sure we  have no null id's
            unsortedList = unsortedList.Where(a => a.Id is not null).ToList();

            var appById = unsortedList.ToDictionary(a => a.Id!);
            var ids = appById.Keys.ToHashSet();

            var missing = dependencies.FirstOrDefault(d => !ids.Contains(d));
            if (missing != null) throw new NetDaemonException(
                $"There is no app named {missing}, please check dependencies or make sure you have not disabled the dependent app!");

            var edges =  unsortedList.SelectMany(p => p.Dependencies.Select(d => (p.Id!, d))).ToHashSet();

            var order = TopologicalSort(ids, edges) ??
                        throw new NetDaemonException(
                            "Application dependencies is wrong, please check dependencies for circular dependencies!");

            return Enumerable.Select<string, INetDaemonApp>(order, id => appById[id]).ToList();
        }

        /// <summary>
        /// Topological Sorting (Kahn's algorithm)
        /// </summary>
        /// <remarks>https://en.wikipedia.org/wiki/Topological_sorting</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes">All nodes of directed acyclic graph.</param>
        /// <param name="edges">All edges of directed acyclic graph.</param>
        /// <returns>Sorted node in topological order.</returns>
        private static List<T>? TopologicalSort<T>(HashSet<T> nodes, HashSet<(T, T)> edges) where T : IEquatable<T>
        {
            // Empty list that will contain the sorted elements
            var L = new List<T>();

            // Set of all nodes with no incoming edges
            var S = new HashSet<T>(nodes.Where(n => edges.All(e => !e.Item2.Equals(n))));

            // while S is non-empty do
            while (S.Count > 0)
            {
                //  remove a node n from S
                var n = S.First();
                S.Remove(n);

                // add n to tail of L
                L.Add(n);

                // for each node m with an edge e from n to m do
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // remove edge e from the graph
                    edges.Remove(e);

                    // if m has no other incoming edges then
                    if (edges.All(me => !me.Item2.Equals(m)))
                    {
                        // insert m into S
                        S.Add(m);
                    }
                }
            }

            // if graph has edges then
            if (edges.Count > 0)
            {
                // return error (graph has at least one cycle)
                return null;
            }
            else
            {
                L.Reverse();
                // return L (a topologically sorted order)
                return L;
            }
        }
    }
}