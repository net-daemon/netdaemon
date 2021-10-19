namespace NetDaemon.Model3.CodeGenerator
{
    public class CodeGenerationSettings
    {
        public string OutputFile { get; init; } = "HomeAssistantGenerated.cs";
        public string Namespace { get; init; } = "HomeAssistantGenerated";
    }
}