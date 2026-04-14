using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http.Extensions;

namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// Options for MConnect Events Consumer.
/// </summary>
public class MConnectEventsConsumerOptions
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
    /// Timeout for connection opening. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Buffer size for receiving data. Defaults to 64 KB.
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 64 * 1024;

    /// <summary>
    /// Specifies whether to consume normal events. Can be disabled to consume test or dead events only. Enabled by default.
    /// </summary>
    public bool ConsumeEvents { get; set; } = true;

    /// <summary>
    /// Specifies whether to consume test events. Enabled by default.
    /// </summary>
    public bool ConsumeTest { get; set; } = true;

    /// <summary>
    /// Specifies whether to consume dead events. Disabled by default.
    /// </summary>
    public bool ConsumeDead { get; set; }

    /// <summary>
    /// The group this consumer belongs to.
    /// Set this when you want to consume the same events in a different consumer group, otherwise, leave it as null.
    /// </summary>
    public string? Group { get; set; }

    public Uri GetConnectUri()
    {
        var query = new QueryBuilder();
        if (!ConsumeEvents) query.Add("events", "false");
        if (!ConsumeTest) query.Add("test", "false");
        if (ConsumeDead) query.Add("dead", "true");

        return new Uri(BaseAddress, new Uri("consume/ws" + query, UriKind.Relative));
    }
}