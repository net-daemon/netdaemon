using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Allows using a Cancellation Token as if it were a task.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be canceled, but never completed.</returns>
        public static Task AsTask(this CancellationToken cancellationToken)
        {
            return AsTask<object>(cancellationToken);
        }

        /// <summary>Allows using a Cancellation Token as if it were a task.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be canceled, but never completed.</returns>
        public static Task<T> AsTask<T>(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            return tcs.Task;
        }

        public static object ParseDataType(string state)
        {
            if (Int64.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out Int64 intValue))
                return intValue;

            if (Double.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out Double doubleValue))
                return doubleValue;

            return state;
        }

        /// <summary>
        ///     Converts HassState to DaemonState
        /// </summary>
        /// <param name="hassState"></param>
        /// <returns></returns>
        public static EntityState ToDaemonEntityState(this HassState hassState)
        {
            var entityState = new EntityState()
            {
                EntityId = hassState.EntityId,
                State = hassState.State,

                LastUpdated = hassState.LastUpdated,
                LastChanged = hassState.LastChanged
            };

            if (hassState.Attributes == null) return entityState;

            // Cast so we can work with the expando object
            var dict = entityState.Attribute as IDictionary<string, object>;

            if (dict == null) throw new ArgumentNullException(nameof(dict),
                "Expando object should always be dictionary!");

            foreach (var (key, value) in hassState.Attributes)
            {
                if (value is JsonElement elem)
                {
                    var dynValue = elem.ToDynamicValue();

                    if (dynValue != null)
                        dict[key] = dynValue;
                }
                else
                {
                    dict[key] = value;
                }
            }

            return entityState;
        }

        public static object? ToDynamicValue(this JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.String:
                    return ParseDataType(elem.GetString());

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.Number:
                    long retVal;
                    if (elem.TryGetInt64(out retVal))
                    {
                        return retVal;
                    }
                    return elem.GetDouble();

                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach (var val in elem.EnumerateArray())
                    {
                        list.Add(val.ToDynamicValue());
                    }
                    return (IEnumerable<object?>)list;

                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object?>();

                    foreach (var prop in elem.EnumerateObject())
                    {
                        obj[prop.Name] = prop.Value.ToDynamicValue();
                    }
                    return (IDictionary<string, object?>)obj;
            }

            return null;
        }

        /// <summary>
        /// A version of Task.WhenAll that can be canceled.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task WhenAll(this IEnumerable<Task> tasks, CancellationToken cancellationToken)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks), $"{nameof(tasks)} is null.");

            await Task.WhenAny(Task.WhenAll(tasks), cancellationToken.AsTask()).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
        // public static dynamic ToDynamic(this (string name, object val)[] attributeNameValuePair)
        // {
        //     // Convert the tuple name/value pair to tuple that can be serialized dynamically
        //     var attributes = new FluentExpandoObject(true, true);
        //     foreach (var (attribute, value) in attributeNameValuePair)
        //     {
        //         ((IDictionary<string, object>)attributes).Add(attribute, value);
        //     }

        //     dynamic result = attributes;
        //     return result;
        // }
    }
}