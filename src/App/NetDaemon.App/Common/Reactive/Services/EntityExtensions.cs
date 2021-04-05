using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDaemon.Common.Reactive.Services
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Rens
        /// </summary>
        /// <typeparam name="TEntityState"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IObservable<(TEntityState Old, TEntityState New)>
            StateAllChanges<TEntityState>(this IEnumerable<RxEntityBase<TEntityState>> entities) 
            where TEntityState : IEntityProperties 
            => entities.Select(t => t.StateAllChanges).Merge();


        public static IObservable<(TEntity Entity, TEntityState Old, TEntityState New)>
            StateAllChangesEx<TEntity, TEntityState>(this IEnumerable<TEntity> entities)
            where TEntity : RxEntityBase<TEntityState>
            where TEntityState : IEntityProperties 
            => entities.Select(t => t.StateAllChanges.Select(e => (t, e.Old, e.New))).Merge();

        public static IObservable<(TProperties Old, TProperties New)> SwitchedOn<TProperties>(
            this IObservable<(TProperties Old, TProperties New)> source)
            where TProperties : IEntityProperties
            => source.Where(e => e.Old.State != "on" && e.New.State == "on");



        /// <summary>
        /// Filters state change events where a predicate was false in the old state and true in the new state
        /// </summary>
        /// <typeparam name="TProperties"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IObservable<(TProperties Old, TProperties New)> ChangedTo<TProperties>(
                        this IObservable<(TProperties Old, TProperties New)> source,
                        Func<TProperties, bool> predicate)
            where TProperties : IEntityProperties
            => source.Where(e => !predicate(e.Old) && predicate(e.New.State));

    }
}
