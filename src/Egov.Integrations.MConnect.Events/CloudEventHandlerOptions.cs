using System.Diagnostics.CodeAnalysis;
using System.IO.Enumeration;

namespace Egov.Integrations.MConnect.Events;

internal sealed class CloudEventHandlerOptions
{
    private readonly Dictionary<string, HandlerInvoker> _invokers = new();
    private readonly Dictionary<string, HandlerInvoker?> _matchedInvokers = new();

    public void AddHandlerInvoker(string type, HandlerInvoker handlerType) 
        => _invokers.Add(type, handlerType);

    public bool TryGetHandlerInvoker(string type, [NotNullWhen(true)] out HandlerInvoker? handlerInvoker)
    {
        if (_matchedInvokers.TryGetValue(type, out handlerInvoker))
        {
            return handlerInvoker != null;
        }

        handlerInvoker = _invokers.Where(pair => FileSystemName.MatchesSimpleExpression(pair.Key, type))
            .Select(pair => pair.Value).FirstOrDefault();
        _matchedInvokers.TryAdd(type, handlerInvoker);
        return handlerInvoker != null;
    }
}
