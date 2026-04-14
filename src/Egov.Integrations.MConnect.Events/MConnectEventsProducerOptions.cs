using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// Options for MConnect Events Producer.
/// </summary>
public class MConnectEventsProducerOptions
{
    /// <summary>
    /// The base address of MConnect Events service.
    /// </summary>
    public required Uri BaseAddress { get; set; }

    /// <summary>
    /// Explicit service certificate to use.
    /// </summary>
    public X509Certificate2? SystemCertificate { get; set; }

    /// <summary>
    /// Explicit intermediate certificates to use.
    /// </summary>
    public X509Certificate2Collection? SystemCertificateIntermediaries { get; set; }

    /// <summary>
    /// Timeout for produce calls. Defaults to 100 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Serializer options to use when serializing <see cref="CloudEvent"/> data to JSON.
    /// By default, the <see cref="CloudEvent.DefaultJsonSerializerOptions" /> is used.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = CloudEvent.DefaultJsonSerializerOptions;
}