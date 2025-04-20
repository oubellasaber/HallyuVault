using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace HallyuVault.Etl.Crawler
{
    public class DramaPostConverter : JsonConverter<DramaPost>
    {
        public override DramaPost Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            try
            {
                var dramaPost = new DramaPost
                {
                    DramaId = GetRequiredInt(root, "id"),
                    RenderedTitle = GetRequiredString(root, "title", "rendered"),
                    RenderedHtml = GetRequiredString(root, "content", "rendered"),
                    Slug = GetRequiredString(root, "slug"),
                    AddedOnUtc = GetRequiredDateTime(root, "date_gmt"),
                    UpdatedOnUtc = GetRequiredDateTime(root, "modified_gmt")
                };

                return dramaPost;
            }
            catch (Exception ex)
            {
                throw new JsonException("Failed to deserialize DramaPost: " + ex.Message, ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, DramaPost value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("id", value.DramaId);
            writer.WriteStartObject("title");
            writer.WriteString("rendered_title", value.RenderedTitle);
            writer.WriteEndObject();
            writer.WriteString("slug", value.Slug);
            writer.WriteString("date_gmt", value.AddedOnUtc.ToString("o"));
            writer.WriteString("modified_gmt", value.UpdatedOnUtc.ToString("o"));
            writer.WriteStartObject("content");
            writer.WriteString("rendered_html", value.RenderedHtml);
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
