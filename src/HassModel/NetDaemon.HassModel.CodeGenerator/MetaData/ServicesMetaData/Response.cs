
using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record Response()
{
    public bool Optional { get; init; }
}
