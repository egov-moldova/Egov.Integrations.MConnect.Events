namespace Egov.Integrations.MConnect.Events.Tests;

public class MConnectEventsConsumerOptionsTests
{
    [Fact]
    public void GetConnectUri_DefaultOptions_ReturnsExpectedUri()
    {
        // Arrange
        var options = new MConnectEventsConsumerOptions
        {
            BaseAddress = new Uri("https://events.egov.md/")
        };

        // Act
        var uri = options.GetConnectUri();

        // Assert
        Assert.Equal("https://events.egov.md/consume/ws", uri.AbsoluteUri);
    }

    [Fact]
    public void GetConnectUri_AllFlagsSet_ReturnsExpectedUriWithQuery()
    {
        // Arrange
        var options = new MConnectEventsConsumerOptions
        {
            BaseAddress = new Uri("https://events.egov.md/"),
            ConsumeEvents = false,
            ConsumeTest = false,
            ConsumeDead = true
        };

        // Act
        var uri = options.GetConnectUri();

        // Assert
        // Order of query params depends on QueryBuilder, but let's check for presence
        var query = uri.Query;
        Assert.Contains("events=false", query);
        Assert.Contains("test=false", query);
        Assert.Contains("dead=true", query);
    }
}
