global using System;
global using System.Collections.Generic;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Threading.Tasks;
global using System.Reactive.Linq;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using NetDaemon.AppModel.Internal;

// Make the internal visible to test project
[assembly: InternalsVisibleTo("NetDaemon.AppModel.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]