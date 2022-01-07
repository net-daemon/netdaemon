global using System;
global using System.Net.WebSockets;
global using System.Reactive.Linq;
global using System.Reactive.Threading.Tasks;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading.Channels;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using FluentAssertions;
global using Moq;
global using Xunit;
global using NetDaemon.Client.Common;
global using NetDaemon.Client.Common.HomeAssistant.Model;
global using NetDaemon.Client.Internal;
global using NetDaemon.Client.Internal.Helpers;
global using NetDaemon.Client.Internal.Net;
global using NetDaemon.Client.Common.Exceptions;
global using NetDaemon.Client.Internal.Extensions;
global using NetDaemon.Client.Internal.HomeAssistant.Commands;
global using NetDaemon.Client.Common.Settings;
global using NetDaemon.Client.Common.HomeAssistant.Extensions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NetDaemon.Runtime.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]