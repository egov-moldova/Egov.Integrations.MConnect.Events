using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Egov.Integrations.MConnect.Events;

internal static class CloudEventExtensions
{
    private const string MediaTypeJson = MediaTypeNames.Application.Json;
    private const string MediaTypeText = MediaTypeNames.Text.Plain;
    private const string MediaTypeBytes = MediaTypeNames.Application.Octet;

    public static HttpContent ToHttpContent(this CloudEvent cloudEvent, JsonSerializerOptions options)
    {
        var content = cloudEvent.Data switch
        {
            byte[] match => new ByteArrayContent(match),
            ReadOnlyMemory<byte> match => new ReadOnlyMemoryContent(match),
            Stream match => new StreamContent(match),
            HttpContent match => match,
            _ => JsonContent.Create(cloudEvent.Data, options: options)
        };

        var headers = content.Headers;
        if (cloudEvent.DataContentType != null)
        {
            headers.ContentType = cloudEvent.DataContentType;
        }

        headers.Add("ce-specversion", cloudEvent.SpecVersion);
        headers.Add("ce-source", cloudEvent.Source.ToString());
        headers.Add("ce-id", cloudEvent.Id);
        headers.Add("ce-type", cloudEvent.Type);
        if (!string.IsNullOrWhiteSpace(cloudEvent.Subject)) headers.Add("ce-subject", cloudEvent.Subject);
        if (cloudEvent.Time != null) headers.Add("ce-time", TimestampFormatter.Format(cloudEvent.Time.Value));
        if (!string.IsNullOrWhiteSpace(cloudEvent.PartitionKey)) headers.Add("ce-partitionkey", cloudEvent.PartitionKey);

        return content;
    }

    public static JsonObject ToJsonObject(this CloudEvent cloudEvent, JsonSerializerOptions options)
    {
        var jsonObject = new JsonObject
        {
            {"specversion", cloudEvent.SpecVersion},
            {"source", cloudEvent.Source?.ToString() },
            {"id", cloudEvent.Id},
            {"type", cloudEvent.Type}
        };
        if (!string.IsNullOrWhiteSpace(cloudEvent.Subject)) jsonObject.Add("subject", cloudEvent.Subject);
        if (cloudEvent.Time != null) jsonObject.Add("time", TimestampFormatter.Format(cloudEvent.Time.Value));
        if (!string.IsNullOrWhiteSpace(cloudEvent.PartitionKey)) jsonObject.Add("partitionkey", cloudEvent.PartitionKey);

        switch (cloudEvent.Data)
        {
            case byte[] match:
                jsonObject.Add("datacontenttype", MediaTypeBytes);
                jsonObject.Add("data_base64", JsonValue.Create(match));
                break;
            case ReadOnlyMemory<byte> match:
                jsonObject.Add("datacontenttype", MediaTypeBytes);
                jsonObject.Add("data_base64", JsonValue.Create(match));
                break;
            case Stream match:
                jsonObject.Add("datacontenttype", MediaTypeBytes);
                using (var reader = new BinaryReader(match))
                {
                    jsonObject.Add("data_base64", JsonValue.Create(reader.ReadBytes((int)match.Length)));
                }
                break;
            case HttpContent match:
                if (match.Headers.ContentType != null)
                {
                    jsonObject.Add("datacontenttype", match.Headers.ContentType?.MediaType);
                }
                jsonObject.Add("data_base64", JsonValue.Create(match.ReadAsByteArrayAsync().Result));
                break;
            default:
                jsonObject.Add("datacontenttype", MediaTypeJson);
                jsonObject.Add("data", JsonSerializer.SerializeToNode(cloudEvent.Data, options));
                break;
        }

        return jsonObject;
    }

    public static CloudEvent? ToCloudEvent(this JsonElement element)
    {
        var cloudEvent = new CloudEvent();
        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "source":
                    if (!Uri.TryCreate(property.Value.GetString(), UriKind.RelativeOrAbsolute, out var source))
                    {
                        return null;
                    }
                    cloudEvent.Source = source;
                    break;
                case "id":
                    cloudEvent.Id = property.Value.GetString()!;
                    break;
                case "type":
                    cloudEvent.Type = property.Value.GetString()!;
                    break;
                case "subject":
                    cloudEvent.Subject = property.Value.GetString();
                    break;
                case "time":
                    cloudEvent.Time = property.Value.GetDateTimeOffset();
                    break;
                case "sequence":
                    cloudEvent.Sequence = property.Value.ValueKind == JsonValueKind.Number ? property.Value.GetInt32() : Int32.Parse(property.Value.GetString()!);
                    break;
                case "offset":
                    cloudEvent.Offset = property.Value.GetString();
                    break;
                case "datacontenttype":
                    cloudEvent.DataContentType = MediaTypeHeaderValue.Parse(property.Value.GetString()!);
                    break;
            }
        }

        if (element.TryGetProperty("data", out var dataElement))
        {
            if (cloudEvent.DataContentType?.MediaType is not MediaTypeJson and not MediaTypeText)
            {
                return null;
            }
            cloudEvent.Data = dataElement;
        }

        if (element.TryGetProperty("data_base64", out var dataBase64Element))
        {
            cloudEvent.Data = dataBase64Element.GetBytesFromBase64();
        }

        return cloudEvent;
    }
}