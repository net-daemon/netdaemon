
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Daemon;

namespace NetDaemon.Service.Api
{

    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        private readonly NetDaemonHost? _host;
        public ApiController(ILoggerFactory? loggerFactory = null, NetDaemonHost? host = null) //, 
        {
            _logger = loggerFactory.CreateLogger<ApiController>();
            _host = host;

        }

        // [Route("api/apps")]
        // [HttpGet]
        // public IEnumerable<INetDaemonAppBase>? Apps()
        // {
        //     return _host?.AllAppInstances;
        // }

        [HttpGet]
        [Route("apps")]
        public IEnumerable<ApiApplication>? Apps()
        {
            return _host?.AllAppInstances.Select(n => new ApiApplication()
            {
                Id = n.Id,
                Dependencies = n.Dependencies,
                IsEnabled = n.IsEnabled
            });
        }
    }

}