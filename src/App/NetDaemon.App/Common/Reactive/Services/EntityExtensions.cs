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
        public static IObservable<(TEntityState Old, TEntityState New)> StateAllChanges<TEntityState>(this IEnumerable<RxEntityBase<TEntityState>> entities) where TEntityState : EntityState
        {
            return entities.Select(t => t.TypedStateAllChanges).Merge();
        }


    }
}
