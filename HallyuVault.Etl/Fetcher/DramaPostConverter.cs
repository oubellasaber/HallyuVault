using HallyuVault.Etl.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HallyuVault.Etl.Fetcher
{
    public class DramaPostConverter : JsonConverter<ScrapedDrama>
    {
        public override ScrapedDrama Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            try
            {
                var dramaPost = new ScrapedDrama
                {
                    ScrapedDramaId = GetRequiredInt(root, "id"),
                    AddedOnUtc = GetRequiredDateTime(root, "date_gmt"),
                    UpdatedOn = GetRequiredDateTime(root, "modified"),
                    UpdatedOnUtc = GetRequiredDateTime(root, "modified_gmt")
                };

                return dramaPost;
            }
            catch (Exception ex)
            {
                throw new JsonException("Failed to deserialize ScrapedDrama: " + ex.Message, ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, ScrapedDrama value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("id", value.ScrapedDramaId);
            writer.WriteString("added_on_utc", value.AddedOnUtc.ToString("o"));
            writer.WriteString("updated_on_utc", value.UpdatedOnUtc.ToString("o"));
            writer.WriteString("updated_on", value.UpdatedOnUtc.ToString("o"));
            writer.WriteString("pulled_on_utc", value.PulledOn.ToString("o"));
            writer.WriteEndObject();
        }

        private int GetRequiredInt(JsonElement parent, string name)
        {
            if (parent.TryGetProperty(name, out var prop) && prop.ValueKind != JsonValueKind.Null)
                return prop.GetInt32();

            throw new InvalidOperationException($"Property '{name}' is missing or null.");
        }

        private DateTime GetRequiredDateTime(JsonElement parent, string name)
        {
            if (parent.TryGetProperty(name, out var prop) && prop.ValueKind != JsonValueKind.Null)
                return prop.GetDateTime();

            throw new InvalidOperationException($"Property '{name}' is missing or null.");
        }

        private string GetRequiredString(JsonElement parent, string name)
        {
            if (parent.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString()!;

            throw new InvalidOperationException($"Property '{name}' is missing or null.");
        }

        private string GetRequiredString(JsonElement parent, string outer, string inner)
        {
            if (parent.TryGetProperty(outer, out var outerProp) &&
                outerProp.TryGetProperty(inner, out var innerProp) &&
                innerProp.ValueKind == JsonValueKind.String)
            {
                return innerProp.GetString()!;
            }

            throw new InvalidOperationException($"Property '{outer}.{inner}' is missing or null.");
        }
    }

}
