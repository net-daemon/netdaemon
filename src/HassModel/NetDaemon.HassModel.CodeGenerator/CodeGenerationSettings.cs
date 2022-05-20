namespace NetDaemon.HassModel.CodeGenerator;

public class CodeGenerationSettings
{
    public string OutputFile { get; init; } = "HomeAssistantGenerated.cs";
    public string OutputFolder { get; init; } = string.Empty;
    public string Namespace { get; init; } = "HomeAssistantGenerated";
    public bool GenerateOneFilePerEntity { get; set; }
}