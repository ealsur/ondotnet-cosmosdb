using System;
using System.IO;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace episode1
{
    public class TextJsonSerializer : CosmosSerializer
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions()
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false, 
            DefaultBufferSize = 1024
        };

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                ReadOnlySpan<byte> span;
                MemoryStream memoryStream = stream as MemoryStream;
                if (memoryStream != null)
                {
                    span = new ReadOnlySpan<byte>(memoryStream.ToArray());
                }
                else
                {
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }

                        span = new ReadOnlySpan<byte>(ms.ToArray());
                    }
                    
                }
                
                return JsonSerializer.Deserialize<T>(span, TextJsonSerializer.options);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            using Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(streamPayload);
            JsonSerializer.Serialize<T>(utf8JsonWriter, input, TextJsonSerializer.options);
            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}