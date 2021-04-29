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

        public CodeGenerationHandler(ICodeGenerator codeGenerator)
        {
            _codeGenerator = codeGenerator;
        }

        public async Task GenerateEntitiesAsync(NetDaemonHost daemonHost, string sourceFolder)
        {
            if (daemonHost == null) throw new ArgumentNullException(nameof(daemonHost));
            if (sourceFolder == null) throw new ArgumentNullException(nameof(sourceFolder));

            var services = await daemonHost.GetAllServices().ConfigureAwait(false);
            var sourceRx = _codeGenerator.GenerateCodeRx(
                    "NetDaemon.Generated.Reactive",
                    daemonHost.State.Select(n => n.EntityId).Distinct(),
                    services
            );

            await File.WriteAllTextAsync(Path.Combine(sourceFolder, "_EntityExtensionsRx.cs.gen"), sourceRx).ConfigureAwait(false);
        }
    }
}
