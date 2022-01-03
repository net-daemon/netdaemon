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
// HassClient usings
global using NetDaemon.Client.Common;
global using NetDaemon.Client.Common.HomeAssistant.Model;
global using NetDaemon.Client.Internal;
global using NetDaemon.Client.Internal.Helpers;
global using NetDaemon.Client.Internal.Json;
global using NetDaemon.Client.Internal.Net;
global using NetDaemon.Client.Common.Exceptions;
global using NetDaemon.Client.Internal.Extensions;
global using NetDaemon.Client.Internal.HomeAssistant.Commands;
global using NetDaemon.Client.Internal.HomeAssistant.Messages;
global using NetDaemon.Client.Common.Settings;

// Make the internal visible to test project
[assembly: InternalsVisibleTo("NetDaemon.HassClient.Tests")]
[assembly: InternalsVisibleTo("NetDaemon.HassModel.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]