using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Services
{
    public  class CoverEntity : RxEntityBase
    {

        public CoverEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void OpenCover(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "open_cover", serviceData);
        }

        public void CloseCover(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "close_cover", serviceData);
        }

        public void SetCoverPosition(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "set_cover_position", serviceData);
        }

        public void StopCover(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "stop_cover", serviceData);
        }

        public void OpenCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "open_cover_tilt", serviceData);
        }

        public void CloseCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "close_cover_tilt", serviceData);
        }

        public void StopCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "stop_cover_tilt", serviceData);
        }

        public void SetCoverTiltPosition(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "set_cover_tilt_position", serviceData);
        }

        public void ToggleCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "toggle_cover_tilt", serviceData);
        }
    }
}