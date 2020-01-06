using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace episode1
{
    public class TextJsonSerializer : CosmosSerializer
    {
        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };
        private const int UnseekableStreamInitialRentSize = 4096;

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
                if (stream.CanSeek
                    && stream.Length == 0)
                {
                    return default(T);
                }

                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                return TextJsonSerializer.Deserialize<T>(stream, options);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            using Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(streamPayload, new JsonWriterOptions() { Indented = options.WriteIndented });
            JsonSerializer.Serialize<T>(utf8JsonWriter, input, options);
            streamPayload.Position = 0;
            return streamPayload;
        }

        /// <summary>
        /// Throughput and allocations optimized deserialization.
        /// </summary>
        /// <remarks>Based off JsonDocument.ReadToEnd https://github.com/dotnet/runtime/blob/master/src/libraries/System.Text.Json/src/System/Text/Json/Document/JsonDocument.Parse.cs#L577. </remarks>
        internal static T Deserialize<T>(
            Stream stream,
            JsonSerializerOptions jsonSerializerOptions)
        {
            int written = 0;
            byte[] rented = null;

            ReadOnlySpan<byte> utf8Bom = TextJsonSerializer.Utf8Bom;

            try
            {
                if (stream.CanSeek)
                {
                    // Ask for 1 more than the length to avoid resizing later,
                    // which is unnecessary in the common case where the stream length doesn't change.
                    long expectedLength = Math.Max(utf8Bom.Length, stream.Length - stream.Position) + 1;
                    rented = ArrayPool<byte>.Shared.Rent(checked((int)expectedLength));
                }
                else
                {
                    rented = ArrayPool<byte>.Shared.Rent(TextJsonSerializer.UnseekableStreamInitialRentSize);
                }

                int lastRead;

                // Read up to 3 bytes to see if it's the UTF-8 BOM
                do
                {
                    lastRead = stream.Read(
                        rented,
                        written,
                        utf8Bom.Length - written);

                    written += lastRead;
                }
                while (lastRead > 0 && written < utf8Bom.Length);

                // If we have 3 bytes, and they're the BOM, reset the write position to 0.
                if (written == utf8Bom.Length &&
                    utf8Bom.SequenceEqual(rented.AsSpan(0, utf8Bom.Length)))
                {
                    written = 0;
                }

                do
                {
                    if (rented.Length == written)
                    {
                        byte[] toReturn = rented;
                        rented = ArrayPool<byte>.Shared.Rent(checked(toReturn.Length * 2));
                        Buffer.BlockCopy(toReturn, 0, rented, 0, toReturn.Length);
                        // Holds document content, clear it.
                        ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
                    }

                    lastRead = stream.Read(rented, written, rented.Length - written);
                    written += lastRead;
                }
                while (lastRead > 0);

                return JsonSerializer.Deserialize<T>(rented.AsSpan(0, written), jsonSerializerOptions);
            }
            finally
            {
                if (rented != null)
                {
                    // Holds document content, clear it before returning it.
                    rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }
    }
}