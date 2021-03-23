using System.Collections.Generic;

using System;
using System.Runtime.CompilerServices;
using System.Linq;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    /// Extensions to provide additional features to groups of typed enitites.
    /// </summary>
    public static class EntityGroupExtensions
    {
        /// <summary>
        /// Turn all entities in the list off. Checks if the entity is on before turning it off.
        /// </summary>
        /// <param name="entities">List of entities to Turn Off</param>
        public static void TurnAllOff(this IEnumerable<SwitchEntity> entities)
        {
            if (entities != null)
            {
                foreach (SwitchEntity entity in entities)
                {
                    if (entity.IsOn)
                    {
                        entity.TurnOff();
                    }

                }
            }

        }
        /// <summary>
        /// Turn all entities in the list on. Checks if the entity is off before turning it on.
        /// </summary>
        /// <param name="entities">List of entities to Turn Off</param>
        public static void TurnAllOn(this IEnumerable<SwitchEntity> entities)
        {
            if (entities != null)
            {
                foreach (SwitchEntity entity in entities)
                {
                    if (entity.IsOff)
                    {
                        entity.TurnOn();
                    }

                }
            }

        }

        /// <summary>
        /// Check if all entities are on
        /// </summary>
        /// <param name="entities">List of entities</param>
        public static bool AreAllOn(this List<SwitchEntity> entities) => entities.All(e => e.IsOn);


        /// <summary>
        /// Check if all entities are off
        /// </summary>
        /// <param name="entities">List of entities</param>
        public static bool AreAllOff(this List<SwitchEntity> entities)  => entities.All(e => e.IsOn);


        /// <summary>
        /// Check if all entities are on
        /// </summary>
        /// <param name="entities">List of entities</param>
        public static bool AreAllOn(this List<BinarySensorEntity> entities) => entities.All(e => e.IsOn);


        /// <summary>
        /// Check if all entities are off
        /// </summary>
        /// <param name="entities">List of entities</param>
        public static bool AreAllOff(this List<BinarySensorEntity> entities)  => entities.All(e => e.IsOn);

        /// <summary>
        /// Subscribe to all changes in list
        /// </summary>
        /// <param name="entities">all entities</param>
        /// <param name="app">NetDaemonApp</param>
        public static IObservable<(EntityState Old, EntityState New)> StateAllChanges(this IEnumerable<RxEntityBase> entities, NetDaemonRxApp app)
        {
            var ids = entities.Select(e => e.EntityId).ToArray();
            return app.Entities(ids).StateAllChanges;
        }
        /// <summary>
        /// Subscribe to changes in list
        /// </summary>
        /// <param name="entities">all entities</param>
        /// <param name="app">NetDaemonApp</param>
        public static IObservable<(EntityState Old, EntityState New)> StateChanges(this IEnumerable<RxEntityBase> entities, NetDaemonRxApp app)
        {
            var ids = entities.Select(e => e.EntityId).ToArray();
            return app.Entities(ids).StateChanges;
        }
        //EntityState extensions
        /// <summary>
        /// Check if an entities state equals on
        /// </summary>
        /// <param name="entityState">Current Entity State</param>
        /// <returns></returns>
        public static bool IsOn(this EntityState entityState)
        {
            return entityState?.State ?? "Unknown" == "on";
        }
        //EntityState extensions
        /// <summary>
        /// Check if an entities state equals on
        /// </summary>
        /// <param name="entityState">Current Entity State</param>
        /// <returns></returns>
        public static bool IsOff(this EntityState entityState)
        {
            return entityState?.State ?? "Unknown" == "off";
        }

    }
}