namespace Eventual.Infrastructure.Serialization
{
    using System.Text.Json;

    class DefaultSerializer : ISerializer
    {
        JsonSerializerOptions _options;

        public DefaultSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
                IgnoreNullValues = true, 
                WriteIndented = false
            };
        }

        public T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content, _options);
        }

        public string Serialize(object content)
        {
            return JsonSerializer.Serialize(content, _options);
        }
    }
}