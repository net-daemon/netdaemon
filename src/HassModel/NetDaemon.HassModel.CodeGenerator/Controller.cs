using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NetDaemon.Client.Settings;

namespace NetDaemon.HassModel.CodeGenerator;

#pragma warning disable CA1303
#pragma warning disable CA2000 // because of await using ... configureAwait()

internal class Controller
{
    private const string ResourceName = "NetDaemon.HassModel.CodeGenerator.MetaData.DefaultMetadata.DefaultEntityMetaData.json";
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

        await Save(mergedEntityMetaData, EntityMetaDataFileName).ConfigureAwait(false);
        await Save(servicesMetaData, ServicesMetaDataFileName).ConfigureAwait(false);

        var generatedTypes = Generator.GenerateTypes(mergedEntityMetaData.Domains, ServiceMetaDataParser.Parse(servicesMetaData!.Value));

        SaveGeneratedCode(generatedTypes);
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

            Console.WriteLine($"Generated {generatedTypes.Length} files.");
            Console.WriteLine(OutputFolder);
        }
    }
}