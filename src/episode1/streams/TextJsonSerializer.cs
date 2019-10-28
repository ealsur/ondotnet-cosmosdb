using System;
using System.IO;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace episode1
{
    public class TextJsonSerializer : CosmosSerializer
    {
        private JsonSerializerOptions options = new JsonSerializerOptions()
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                return JsonSerializer.DeserializeAsync<T>(stream, this.options).GetAwaiter().GetResult();
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream stream = new MemoryStream();
            JsonSerializer.SerializeAsync(stream, input, this.options).GetAwaiter().GetResult();
            return stream;
        }
    }
}