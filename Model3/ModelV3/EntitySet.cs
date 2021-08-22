// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reactive.Linq;
// using System.Text.Json;
// using NetDaemon.Common.ModelV3;
//
// namespace NetDaemon.Common.Model
// {
//     public class EntitySet
//     {
//         /// <summary>
//         ///     The IHAContext
//         /// </summary>
//         protected IHaContext HaContext { get; }
//
//         /// <summary>
//         ///     Entity ids being handled by the RxEntity
//         /// </summary>
//         public IEnumerable<string> EntityIds { get; }
//
//         /// <summary>
//         ///     Constructor
//         /// </summary>
//         /// <param name="daemon">The NetDaemon host object</param>
//         /// <param name="entityIds">Unique entity id:s</param>
//         public EntitySet(IHaContext daemon, IEnumerable<string> entityIds)
//         {
//             HaContext = daemon;
//             EntityIds = entityIds;
//         }
//
//         public virtual void SetState(string state, object? attributes = null, bool waitForResponse = false)
//         {
//             foreach (var entityId in EntityIds)
//             {
//                 //HaContext.SetState(entityId, state, attributes, waitForResponse);
//             }
//         }
//
//         public virtual IObservable<StateChange> StateAllChanges =>
//             HaContext.StateAllChanges.Where(e => EntityIds.Contains(e.New.EntityId))
//                 .Select(e => new StateChange { Entity = new Entity(HaContext, e.New.EntityId), Old = e.Old, New = e.New });
//
//         public IObservable<StateChange> StateChanges =>
//             HaContext.StateChanges.Where(e => EntityIds.Contains(e.New.EntityId))
//                 .Select(e => new StateChange { Entity = new Entity(HaContext, e.New.EntityId), Old = e.Old, New = e.New });
//         
//         public virtual void CallService(string service, object? data = null, bool waitForResponse = false)
//         {
//             var element = JsonSerializer.Serialize(data);
//             var serviceData = JsonSerializer.Deserialize<Dictionary<string, object>>(element);
//             foreach (var entityId in EntityIds!)
//             {
//                 var domain = entityId.SplitEntityId().Domain;
//                 serviceData["entity_id"] = entityId;
//
//                 HaContext.CallService(domain, service, serviceData, waitForResponse);
//             }
//         }
//     }
// }