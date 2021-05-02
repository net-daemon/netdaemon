using System.Collections.Generic;
using JoySoftware.HomeAssistant.Client;

namespace NetDaemon.Service.App
{
    public interface ICodeGenerator
    {
        string? GenerateCodeRx(string nameSpace, IEnumerable<string> entities, IEnumerable<HassServiceDomain> services);
    }
}
