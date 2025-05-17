namespace Backend.Fx.Mediator.Feature.Registry;

internal readonly record struct HandlerKey
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