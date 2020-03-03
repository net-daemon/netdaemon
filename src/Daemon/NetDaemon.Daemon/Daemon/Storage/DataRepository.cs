using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage
{
    public class DataRepository : IDataRepository
    {
        private readonly string _dataStoragePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public DataRepository(string dataStoragePath)
        {
            _dataStoragePath = dataStoragePath;

            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new ExpandoDictionaryConverter());
        }

        /// <inheritdoc/>
        public async ValueTask<T> Get<T>(string id)
        {
            try
            {
                var storageJsonFile = Path.Combine(_dataStoragePath, $"{id}_store.json");

                using var jsonStream = File.OpenRead(storageJsonFile);

                return await JsonSerializer.DeserializeAsync<T>(jsonStream, _jsonOptions);
            }
            catch  // Ignore all errors for now
            {
            }
#pragma warning disable CS8603, CS8653
            return default(T);
#pragma warning restore CS8603, CS8653
        }

        /// <inheritdoc/>
        public async Task Save<T>(string id, T data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var storageJsonFile = Path.Combine(_dataStoragePath, $"{id}_store.json");

            if (!Directory.Exists(_dataStoragePath))
            {
                Directory.CreateDirectory(_dataStoragePath);
            }

            using var jsonStream = File.Open(storageJsonFile, FileMode.Create, FileAccess.Write);

            await JsonSerializer.SerializeAsync<T>(jsonStream, data);
        }
    }

    public class ExpandoDictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader);
            var returnObject = new Dictionary<string, object>();
            var returnDict = (IDictionary<string, object>)returnObject;
            foreach (var x in dict.Keys)
            {
                if (dict[x] is JsonElement jsonElem)
                {
                    returnDict[x] = jsonElem!.ToObjectValue() ?? dict[x];
                }
            }
            return returnObject;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, object> value,
            JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<Dictionary<string, object>>(writer, value, options);
        }
    }

    public static class ExpandoExtensions
    {
        public static object ParseString(string strToParse)
        {
            if (DateTime.TryParse(strToParse, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                return dateTime;

            // Just a normal string
            return strToParse;
        }

        public static object? ToObjectValue(this JsonElement elem)
        {
            return elem.ValueKind switch
            {
                JsonValueKind.String => ParseString(elem.GetString()),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Number => elem.TryGetInt64(out Int64 intValue) ? intValue : elem.GetDouble(),
                _ => null
            };
        }
    }
}