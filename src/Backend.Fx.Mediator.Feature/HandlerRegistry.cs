using System.Collections.Concurrent;

namespace Backend.Fx.Mediator.Feature;

public readonly record struct HandlerKey
{
    public static HandlerKey For<TNotification>() => new (typeof(TNotification), typeof(void));
    public static HandlerKey For<TRequest, TResponse>() where TRequest : IRequest<TResponse> 
        => new (typeof(TRequest), typeof(TResponse));
    
    public HandlerKey(Type RequestType, Type? ResponseType = null)
    {
        this.RequestType = RequestType;
        this.ResponseType = ResponseType ?? typeof(void);
    }

    public Type RequestType { get; init; }
    public Type ResponseType { get; init; }
}

public interface IHandlerRegistry
{
    IEnumerable<Type> GetHandlerTypes(HandlerKey key);
}

public class HandlerRegistry : IHandlerRegistry
{
    private readonly ConcurrentDictionary<HandlerKey, List<Type>> _handlers = new();

    public IEnumerable<Type> GetHandlerTypes(HandlerKey key)
    {
        return _handlers.GetOrAdd(key, _ => []);
    }

    public void Add(HandlerKey key, Type handlerType)
    {
        _handlers.GetOrAdd(key, _ => []).Add(handlerType);
    }
}