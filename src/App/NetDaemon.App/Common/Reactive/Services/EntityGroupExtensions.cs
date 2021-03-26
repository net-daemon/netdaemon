using System.Collections.Generic;

using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;


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
        public static void TurnOff(this IEnumerable<SwitchEntity> entities)
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
        public static void TurnOn(this IEnumerable<SwitchEntity> entities)
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
        public static bool AreOn(this List<SwitchEntity> entities) => entities.All(e => e.IsOn);


        /// <summary>
        /// Check if all entities are off
        /// </summary>
        /// <param name="entities">List of entities</param>
        public static bool AreOff(this List<SwitchEntity> entities) => entities.All(e => e.IsOn);


        /// <summary>
        /// Check if all entities are on
        /// </summary>
        /// <param name="entities">List of entities</param>
        public static bool AreOn(this List<BinarySensorEntity> entities) => entities.All(e => e.IsOn);


        /// <summary>
        /// Check if all entities are off
        /// </summary>
        /// <param name="entities">List of entities</param>
        public static bool AreOff(this List<BinarySensorEntity> entities) => entities.All(e => e.IsOn);

        /// <summary>
        /// Subscribe to all changes in a Sun Entity list
        /// </summary>
        /// <param name="entities">all entities</param>
        public static IObservable<(SunEntityState Old, SunEntityState New)> StateAllChanges(this IEnumerable<SunEntity> entities)
        {
            return entities.Select(t => t.StateAllChanges).Merge();
        }

        /// <summary>
        /// Subscribe to changes in a Sun Entity list
        /// </summary>
        /// <param name="entities">all entities</param>
        public static IObservable<(SunEntityState Old, SunEntityState New)> StateChanges(this IEnumerable<SunEntity> entities)
        {

            return entities.Select(t => t.StateChanges).Merge();
        }

        /// <summary>
        /// Subscribe to all changes in a Switch Entity list
        /// </summary>
        /// <param name="entities">all entities</param>
        public static IObservable<(SwitchEntityState Old, SwitchEntityState New)> StateAllChanges(this IEnumerable<SwitchEntity> entities)
        {
            return entities.Select(t => t.StateAllChanges).Merge();
        }

        /// <summary>
        /// Subscribe to changes in a Switch Entity list
        /// </summary>
        /// <param name="entities">all entities</param>
        public static IObservable<(SwitchEntityState Old, SwitchEntityState New)> StateChanges(this IEnumerable<SwitchEntity> entities)
        {

            return entities.Select(t => t.StateChanges).Merge();
        }



        /// <summary>
        /// Subscribe to all changes in a Binary Sensor Entity list
        /// </summary>
        /// <param name="entities">all entities</param>
        public static IObservable<(BinaryEntityState Old, BinaryEntityState New)> StateAllChanges(this IEnumerable<BinarySensorEntity> entities)
        {
            return entities.Select(t => t.StateAllChanges).Merge();
        }

        /// <summary>
        /// Subscribe to changes in a Binary Sensor Entity list
        /// </summary>
        /// <param name="entities">all entities</param>
        public static IObservable<(BinaryEntityState Old, BinaryEntityState New)> StateChanges(this IEnumerable<BinarySensorEntity> entities)
        {

            return entities.Select(t => t.StateChanges).Merge();
        }



    }
}