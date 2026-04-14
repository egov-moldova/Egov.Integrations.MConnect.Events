using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// Represents a CloudEvent.
/// </summary>
public class CloudEvent
{
    /// <summary>
    /// The default <see cref="JsonSerializerOptions"/> used for serialization and deserialization of <see cref="CloudEvent"/> data.<br/>
    /// The options are configured with the following:<br/>
    /// - Property names are treated as case-insensitive<br/>
    /// - "camelCase" name formatting should be employed<br/>
    /// - <see langword="null" /> values are not written to the output<br/>
    /// - Quoted numbers (JSON strings for number properties) are allowed.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// The CloudEvents specification version for this event.
    /// </summary>
    public string SpecVersion => "1.0";

    /// <summary>
    /// CloudEvents <see href="https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#source">'source'</see> attribute.
    /// This describes the event producer. Often this will include information such as the type of the event source, the
    /// organization producing the event, the process that produced the event, and some unique identifiers.
    /// If not set, MConnect Events will set it to the identity of calling client.
    /// When combined with <see cref="Id"/>, this enables deduplication.
    /// </summary>
    public Uri Source { get; set; }

    /// <summary>
    /// CloudEvent <see href="https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#id">'id'</see> attribute,
    /// This is the ID of the event. When combined with <see cref="Source"/>, this enables deduplication.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// CloudEvents <see href="https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#type">'type'</see> attribute.
    /// Type of occurrence which has happened.
    /// Often this attribute is used for routing, observability, policy enforcement, etc.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// CloudEvents <see href="https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#subject">'subject'</see> attribute.
    /// This describes the subject of the event in the context of the event producer (identified by <see cref="Source"/>).
    /// In publish-subscribe scenarios, a subscriber will typically subscribe to events emitted by a source,
    /// but the source identifier alone might not be sufficient as a qualifier for any specific event if the source context has
    /// internal sub-structure.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// CloudEvents <see href="https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#time">'time'</see> attribute.
    /// Timestamp of when the occurrence happened.
    /// </summary>
    public DateTimeOffset? Time { get; set; }

    /// <summary>
    /// Support for the <see href="https://github.com/cloudevents/spec/tree/main/cloudevents/extensions/partitioning.md">partitioning</see>
    /// CloudEvent extension. Use when producing events to ensure events ordering by this key.
    /// </summary>
    public string? PartitionKey { get; set; }

    /// <summary>
    /// Support for the <see href="https://github.com/cloudevents/spec/tree/main/cloudevents/extensions/sequence.md">sequence</see>
    /// CloudEvent extension. Used to describe the position of an event in the ordered sequence of events produced by a source.
    /// </summary>
    public int? Sequence { get; set; }

    /// <summary>
    /// Support for custom offset attribute used by consumers to confirm event consumption.
    /// </summary>
    public string? Offset { get; set; }

    /// <summary>
    /// CloudEvent <see href="https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#datacontenttype">'datacontenttype'</see> attribute.
    /// This is the content type of the <see cref="Data"/> property.
    /// This attribute enables the data attribute to carry any type of content,
    /// where the format and encoding might differ from that of the chosen event format.
    /// </summary>
    public MediaTypeHeaderValue? DataContentType { get; set; }

    /// <summary>
    /// CloudEvent 'data' content. The event payload. The payload depends on the 'type'.
    /// It is encoded into a media format which is specified by the 'datacontenttype' attribute (e.g. application/json).
    /// </summary>
    /// <see href="https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#event-data"/>
    public object? Data { get; set; }

    /// <summary>
    /// Converts event's binary data into a byte array.
    /// </summary>
    /// <returns>A byte array representation of the binary data.</returns>
    public byte[]? DeserializeBinaryData()
    {
        return Data as byte[];
    }

    /// <summary>
    /// Converts event's data representing a single JSON value into a <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">The type of the event data to deserialize into.</typeparam>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <returns>A <typeparamref name="TData"/> representation of the JSON data.</returns>
    public TData? DeserializeData<TData>(JsonSerializerOptions? options = null)
    {
        if (Data is not JsonElement element) return default;
        return element.Deserialize<TData>(options ?? DefaultJsonSerializerOptions);
    }
}