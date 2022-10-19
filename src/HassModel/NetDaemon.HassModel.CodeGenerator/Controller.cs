using System.IO;
using System.Threading.Tasks;
using NetDaemon.Client.Settings;

namespace NetDaemon.HassModel.CodeGenerator;

#pragma warning disable CA1303
#pragma warning disable CA2000 // because of await using ... configureAwait()

internal class Controller
{
    private readonly CodeGenerationSettings _generationSettings;
    private readonly HomeAssistantSettings _haSettings;

    public Controller(CodeGenerationSettings generationSettings, HomeAssistantSettings haSettings)
    {
        _generationSettings = generationSettings;
        _haSettings = haSettings;
    }

    private string EntityMetaDataFileName => Path.Combine(OutputFolder, "EntityMetaData.json");
    private string ServicesMetaDataFileName => Path.Combine(OutputFolder, "ServicesMetaData.json");

    private string OutputFolder => string.IsNullOrEmpty(_generationSettings.OutputFolder) 
        ? Directory.GetParent(Path.GetFullPath(_generationSettings.OutputFile))!.FullName 
        : _generationSettings.OutputFolder;
    
    public async Task RunAsync()
    {
        var (hassStates, servicesMetaData) = await HaRepositry.GetHaData(_haSettings).ConfigureAwait(false);

        var previousEntityMetadata = await LoadEntitiesMetaDataAsync().ConfigureAwait(false);
        var currentEntityMetaData = EntityMetaDataGenerator.GetEntityDomainMetaData(hassStates);
        var mergedEntityMetaData = EntityMetaDataMerger.Merge(_generationSettings, previousEntityMetadata, currentEntityMetaData);

        var generatedTypes = Generator.GenerateTypes(mergedEntityMetaData.Domains, servicesMetaData!.Value.ToServicesResult() );

        await Save(mergedEntityMetaData, EntityMetaDataFileName).ConfigureAwait(false);
        await Save(servicesMetaData, ServicesMetaDataFileName).ConfigureAwait(false);
        
        SaveGeneratedCode(generatedTypes.ToList());
    }

    private async Task<EntitiesMetaData> LoadEntitiesMetaDataAsync()
    {
        if (!File.Exists(EntityMetaDataFileName)) return new EntitiesMetaData();
        
        var fileStream = File.OpenRead(EntityMetaDataFileName);
        await using var _ = fileStream.ConfigureAwait(false);

        var loaded = await JsonSerializer.DeserializeAsync<EntitiesMetaData>(fileStream, JsonSerializerOptions).ConfigureAwait(false);

        return loaded ?? new EntitiesMetaData();
    }

    private async Task Save<T>(T merged, string fileName)
    {
        Directory.CreateDirectory(OutputFolder);

        var fileStream = File.OpenWrite(fileName);
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
    
    private void SaveGeneratedCode(IReadOnlyCollection<MemberDeclarationSyntax> generatedTypes)
    {
        if (!_generationSettings.GenerateOneFilePerEntity)
        {
            Console.WriteLine("Generating single file for all entities.");
            var unit = Generator.BuildCompilationUnit(_generationSettings.Namespace, generatedTypes.ToArray());

            Directory.CreateDirectory(Directory.GetParent(_generationSettings.OutputFile)!.FullName);

            using var writer = new StreamWriter(_generationSettings.OutputFile);
            unit.WriteTo(writer);

            Console.WriteLine(Path.GetFullPath(_generationSettings.OutputFile));
        }
        else
        {
            Console.WriteLine("Generating separate file per entity.");

            Directory.CreateDirectory(OutputFolder);

            foreach (var type in generatedTypes)
            {
                var unit = Generator.BuildCompilationUnit(_generationSettings.Namespace, type);
                using var writer = new StreamWriter(Path.Combine(OutputFolder, $"{unit.GetClassName()}.cs"));
                unit.WriteTo(writer);
            }

            Console.WriteLine($"Generated {generatedTypes.Count} files.");
            Console.WriteLine(OutputFolder);
        }
    }
}