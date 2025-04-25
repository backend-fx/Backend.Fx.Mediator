using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature;

public interface IRootMediator : IMediator;

public class RootMediator : IRootMediator
{
    private readonly IMediator _mediator;

    public RootMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public ValueTask DisposeAsync()
    {
        return _mediator.DisposeAsync();
    }

    public void Notify<TNotification>(TNotification notification) where TNotification : class
    {
        _mediator.Notify(notification);
    }

    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
    {
        return _mediator.RequestAsync<TRequest, TResponse>(request, cancellation);
    }

    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, IIdentity requestor,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
    {
        return _mediator.RequestAsync<TRequest, TResponse>(request, requestor, cancellation);
    }
}