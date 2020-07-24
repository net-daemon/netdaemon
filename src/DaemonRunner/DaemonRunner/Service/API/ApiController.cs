
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Daemon;
using NetDaemon.Service.Configuration;

namespace NetDaemon.Service.Api
{

    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly NetDaemonSettings? _netdaemonSettings;
        private readonly HomeAssistantSettings? _homeassistantSettings;

        private readonly NetDaemonHost? _host;
        public ApiController(
            IOptions<NetDaemonSettings> netDaemonSettings,
            IOptions<HomeAssistantSettings> homeAssistantSettings,
            ILoggerFactory? loggerFactory = null,
            NetDaemonHost? host = null
            )
        {
            _logger = loggerFactory.CreateLogger<ApiController>();
            _host = host;
            _netdaemonSettings = netDaemonSettings.Value;
            _homeassistantSettings = homeAssistantSettings.Value;
        }

        [Route("settings")]
        [HttpGet]
        public ApiConfig? Config()
        {
            var tempResult = new ApiConfig
            {
                DaemonSettings = _netdaemonSettings,
                HomeAssistantSettings = _homeassistantSettings
            };
            // For first release we do not expose the token
            if (tempResult.HomeAssistantSettings is object)
            {
                tempResult.HomeAssistantSettings.Token = "";
            }
            return tempResult;
        }

        [HttpGet]
        [Route("apps")]
        public IEnumerable<ApiApplication>? Apps()
        {

            return _host?.AllAppInstances.Select(n => new ApiApplication()
            {
                Id = n.Id,
                Dependencies = n.Dependencies,
                IsEnabled = n.IsEnabled,
                Description = n.Description
            });
        }
    }

}