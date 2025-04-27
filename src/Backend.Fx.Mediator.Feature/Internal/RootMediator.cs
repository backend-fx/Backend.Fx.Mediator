using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature.Internal;

public interface IRootMediator : IMediator;

public class RootMediator : IRootMediator
{
    private readonly Mediator _mediator;

    public RootMediator(Mediator mediator)
    {
        _mediator = mediator;
    }

    public ValueTask DisposeAsync() => _mediator.DisposeAsync();

    public ValueTask Notify<TNotification>(TNotification notification, CancellationToken cancellation = default)
        where TNotification : class
        => _mediator.Notify(notification, cancellation);

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default)
        where TResponse : class
        => _mediator.RequestAsync(request, cancellation);

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default)
        where TResponse : class
        => _mediator.RequestAsync(request, requestor, cancellation);

    public void Dispose() => _mediator.Dispose();
}