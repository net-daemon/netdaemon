using System.Reflection;
using NetDaemon.Client.Settings;

namespace NetDaemon.HassModel.CodeGenerator;

#pragma warning disable CA1303
#pragma warning disable CA2000 // because of await using ... configureAwait()

internal class Controller(CodeGenerationSettings generationSettings, HomeAssistantSettings haSettings)
{
    private const string ResourceName = "NetDaemon.HassModel.CodeGenerator.MetaData.DefaultMetadata.DefaultEntityMetaData.json";

    private string EntityMetaDataFileName => Path.Combine(OutputFolder, "EntityMetaData.json");
    private string ServicesMetaDataFileName => Path.Combine(OutputFolder, "ServicesMetaData.json");

    private string OutputFolder => string.IsNullOrEmpty(generationSettings.OutputFolder)
        ? Directory.GetParent(Path.GetFullPath(generationSettings.OutputFile))!.FullName
        : generationSettings.OutputFolder;

    public async Task RunAsync()
    {
        var (hassStates, servicesMetaData) = await HaRepositry.GetHaData(haSettings).ConfigureAwait(false);

        var previousEntityMetadata = await LoadEntitiesMetaDataAsync().ConfigureAwait(false);
        var currentEntityMetaData = EntityMetaDataGenerator.GetEntityDomainMetaData(hassStates);
        var mergedEntityMetaData = EntityMetaDataMerger.Merge(generationSettings, previousEntityMetadata, currentEntityMetaData);

        await Save(mergedEntityMetaData, EntityMetaDataFileName).ConfigureAwait(false);
        await Save(servicesMetaData, ServicesMetaDataFileName).ConfigureAwait(false);

        var hassServiceDomains = ServiceMetaDataParser.Parse(servicesMetaData!.Value, out var deserializationErrors);
        CheckParseErrors(deserializationErrors);

        var generatedTypes = Generator.GenerateTypes(mergedEntityMetaData.Domains, hassServiceDomains);

        SaveGeneratedCode(generatedTypes);
    }

    internal static void CheckParseErrors(List<DeserializationError> parseErrors)
    {
        if (parseErrors.Count == 0) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("""
                          Errors occured while parsing metadata from Home Assistant for one or more services.
                          This is usually caused by metadata from HA that is not in the expected JSON format.
                          nd-codegen will try to continue to generate code for other services.
                          """);
        Console.ResetColor();
        foreach (var deserializationError in parseErrors)
        {
            Console.WriteLine();
            Console.WriteLine(deserializationError.Exception);
            Console.WriteLine(deserializationError.Context + " = ");
            Console.Out.Flush();
            Console.WriteLine(JsonSerializer.Serialize(deserializationError.Element, new JsonSerializerOptions{WriteIndented = true}));
        }
    }

    internal async Task<EntitiesMetaData> LoadEntitiesMetaDataAsync()
    {
        var fileStream = File.Exists(EntityMetaDataFileName) switch
        {
            true => File.OpenRead(EntityMetaDataFileName),
            false => GetDefaultMetaDataFileFromResource()
        };

        await using var _ = fileStream.ConfigureAwait(false);

        var loaded = await JsonSerializer.DeserializeAsync<EntitiesMetaData>(fileStream, JsonSerializerOptions).ConfigureAwait(false);

        return loaded ?? new EntitiesMetaData();
    }

    private static Stream GetDefaultMetaDataFileFromResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(ResourceName)!;
    }

    private async Task Save<T>(T merged, string fileName)
    {
        Directory.CreateDirectory(OutputFolder);

        var fileStream = File.Create(fileName);
        await using var _ = fileStream.ConfigureAwait(false);
        await JsonSerializer.SerializeAsync(fileStream, merged, JsonSerializerOptions).ConfigureAwait(false);
    }

    private static JsonSerializerOptions JsonSerializerOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new ClrTypeJsonConverter() }
        };

    private void SaveGeneratedCode(MemberDeclarationSyntax[] generatedTypes)
    {
        if (!generationSettings.GenerateOneFilePerEntity)
        {
            Console.WriteLine("Generating single file for all entities.");
            var unit = Generator.BuildCompilationUnit(generationSettings.Namespace, generatedTypes.ToArray());

            Directory.CreateDirectory(Directory.GetParent(generationSettings.OutputFile)!.FullName);

            using var writer = new StreamWriter(generationSettings.OutputFile);
            unit.WriteTo(writer);

            Console.WriteLine(Path.GetFullPath(generationSettings.OutputFile));
        }
        else
        {
            Console.WriteLine("Generating separate file per entity.");

            Directory.CreateDirectory(OutputFolder);

            foreach (var type in generatedTypes)
            {
                var unit = Generator.BuildCompilationUnit(generationSettings.Namespace, type);
                using var writer = new StreamWriter(Path.Combine(OutputFolder, $"{unit.GetClassName()}.cs"));
                unit.WriteTo(writer);
            }

            Console.WriteLine($"Generated {generatedTypes.Length} files.");
            Console.WriteLine(OutputFolder);
        }
    }
}
