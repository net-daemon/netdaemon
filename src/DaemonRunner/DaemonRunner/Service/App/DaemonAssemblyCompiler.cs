using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Exceptions;
using NetDaemon.Infrastructure.Extensions;

namespace NetDaemon.Service.App
{
    public class DaemonAssemblyCompiler : IDaemonAssemblyCompiler
    {
        private readonly ILogger<DaemonAssemblyCompiler> _logger;

        private readonly string? _sourceFolder;
        
        // public DaemonAssemblyCompiler(ILogger<DaemonAssemblyCompiler> logger, IOptions<NetDaemonSettings> netDaemonSettings)
        public DaemonAssemblyCompiler(ILogger<DaemonAssemblyCompiler> logger, NetDaemonSettings netDaemonSettings)
        {
            _ = netDaemonSettings ??
                throw new NetDaemonArgumentNullException(nameof(netDaemonSettings));
            _logger = logger;
            // NetDaemonSettings = netDaemonSettings;
            // _sourceFolder = netDaemonSettings.Value.GetAppSourceDirectory();
            _sourceFolder = netDaemonSettings.GetAppSourceDirectory();
        }

        public IOptions<NetDaemonSettings> NetDaemonSettings { get; }

        public IEnumerable<Assembly> Load()
        {
            return new []{ DaemonCompiler.GetCompiledAppAssembly(out _, _sourceFolder!, _logger) };
        }
    }
}