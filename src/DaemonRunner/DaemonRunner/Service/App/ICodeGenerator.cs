using System.Collections.Generic;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Service.App
{
    public interface ICodeGenerator
    {
        string? GenerateCodeRx(string nameSpace, IReadOnlyCollection<string> entities, IReadOnlyCollection<HassServiceDomain> services);
    }
}
