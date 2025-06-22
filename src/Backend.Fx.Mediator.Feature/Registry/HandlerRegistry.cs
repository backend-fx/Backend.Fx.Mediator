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
    
    public Type GetRequestHandlerType<TResponse>(Type requestType)
    {
        var key = new HandlerKey(requestType, typeof(TResponse));
        var handlerTypes = GetHandlerTypes(key).ToArray();
        return handlerTypes.SingleOrDefault()
               ?? throw new InvalidOperationException(
                   $"No handler found for request type {requestType.Name} with response type {typeof(TResponse).Name}");
    }

    public Type[] GetNotificationHandlerTypes<TNotification>() where TNotification : class
    {
        var key = HandlerKey.For<TNotification>();
        var handlerTypes = GetHandlerTypes(key).ToArray();
        return handlerTypes;
    }
}