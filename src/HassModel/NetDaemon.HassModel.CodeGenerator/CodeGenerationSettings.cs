using System.Collections.ObjectModel;

namespace NetDaemon.HassModel.CodeGenerator;

public record CodeGenerationSettings
{
    public string OutputFile { get; init; } = "HomeAssistantGenerated.cs";
    public string OutputFolder { get; init; } = string.Empty;
    public string Namespace { get; init; } = "HomeAssistantGenerated";
    public bool UseAttributeBaseClasses { get; set; } // For now we default to false for backwards compat. Later we might default to true
    public bool GenerateOneFilePerEntity { get; set; }
}