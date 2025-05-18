using System.Collections.Concurrent;

namespace Backend.Fx.Mediator.Feature.Registry;

internal class HandlerRegistry
{
    private readonly ConcurrentDictionary<HandlerKey, List<Type>> _handlers = new();

    public IEnumerable<Type> GetHandlerTypes(HandlerKey key)
    {
        return _handlers.GetOrAdd(key, _ => []);
    }

    public IEnumerable<HandlerMetaData> MetaData => _handlers.SelectMany(kvp => kvp.Value.Select(t => new HandlerMetaData(t, kvp.Key.RequestType, kvp.Key.ResponseType)));

    public void Add(HandlerKey key, Type handlerType)
    {
        var handlers = _handlers.GetOrAdd(key, _ => []);
        if (key.ResponseType != typeof(void) && handlers.Count > 0)
        {
            throw new InvalidOperationException(
                $"Handler for {key.RequestType} already registered: {handlers.First().Name}");
        }

        handlers.Add(handlerType);
    }
}