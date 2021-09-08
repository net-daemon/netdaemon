using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetDaemon.Daemon;

namespace NetDaemon.Service.App
{
    public class CodeGenerationHandler : ICodeGenerationHandler
    {
        private readonly ICodeGenerator _codeGenerator;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="codeGenerator">ICodeGenerator instance</param>
        public CodeGenerationHandler(ICodeGenerator codeGenerator)
        {
            _codeGenerator = codeGenerator;
        }

        /// <inheritdoc/>
        public Task GenerateEntitiesAsync(NetDaemonHost daemonHost, string sourceFolder)
        {
            if (daemonHost == null) throw new ArgumentNullException(nameof(daemonHost));
            if (sourceFolder == null) throw new ArgumentNullException(nameof(sourceFolder));

            return GenerateEntitiesAsyncInternal(daemonHost, sourceFolder);
        }

        private async Task GenerateEntitiesAsyncInternal(NetDaemonHost daemonHost, string sourceFolder)
        {
            var services = await daemonHost.GetAllServices().ConfigureAwait(false);
            var entityIds = daemonHost.State.Distinct().ToList();

            var sourceRx = _codeGenerator.GenerateCodeRx(
                    "NetDaemon.Generated.Reactive.Services",
                    entityIds,
                    services.ToList()
            );

            await File.WriteAllTextAsync(Path.Combine(sourceFolder, "_EntityExtensionsRx.cs.gen"), sourceRx).ConfigureAwait(false);
        }
    }
}
