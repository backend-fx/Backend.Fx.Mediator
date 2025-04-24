using System.Security.Principal;
using Backend.Fx.Execution;

namespace Backend.Fx.Mediator.Feature;

public class Mediator : IMediator
{
    private readonly IBackendFxApplication _application;
    private readonly MediatorOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Mediator(IBackendFxApplication application, MediatorOptions options)
    {
        _application = application;
        _options = options;
    }

    public void Notify<TNotification>(TNotification notification) where TNotification : class
    {
        //todo register Task and wait for it to finish on disposal
        _ = _application.NotifyAsync(notification, _options.ErrorHandler, _cancellationTokenSource.Token);
    }


    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        => RequestAsync<TRequest, TResponse>(request, _options.DefaultRequestor, cancellation);

    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, IIdentity requestor,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        => _application.InvokeRequestAsync<TRequest, TResponse>(request, requestor, cancellation);


    public void Dispose()
    {
    }
}