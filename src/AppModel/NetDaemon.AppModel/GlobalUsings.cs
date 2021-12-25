global using System;
global using System.Buffers;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.IO.Pipelines;
global using System.Globalization;
global using System.Net.Http.Headers;
global using System.Net.WebSockets;
global using System.Net.Security;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading.Tasks;
global using System.Reactive.Subjects;
global using System.Reactive.Linq;
global using System.Reactive.Threading.Tasks;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using NetDaemon.AppModel.Common;
global using NetDaemon.AppModel.Internal;
global using NetDaemon.AppModel.Common.TypeResolver;
global using NetDaemon.AppModel.Internal.TypeResolver;

// Make the internal visible to test project
[assembly: InternalsVisibleTo("NetDaemon.AppModel.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]