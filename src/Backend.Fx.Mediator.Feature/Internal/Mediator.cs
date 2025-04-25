using System.Security.Principal;
using Backend.Fx.Execution;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Mediator.Feature.Internal;

public sealed class Mediator : IMediator
{
    private readonly ILogger _logger = Log.Create<Mediator>();
    private readonly IBackendFxApplication _application;
    private readonly MediatorOptions _options;
    private volatile bool _disposing;

    public Mediator(IBackendFxApplication application, MediatorOptions options)
    {
        _application = application;
        _options = options;
    }

    public async ValueTask Notify<TNotification>(TNotification notification, CancellationToken cancellation = default)
        where TNotification : class
    {
        // Don't accept new notifications during disposal
        if (_disposing)
        {
            _logger.LogWarning(
                "Discarding notification {Notification} because the mediator is disposing.",
                notification);
            return;
        }
        
        await _application
            .NotifyAsync(notification, _options.ErrorHandler, cancellation)
            .ConfigureAwait(false);
    }

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default) => RequestAsync(request, _options.DefaultRequestor, cancellation);

    public ValueTask<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, IIdentity requestor,
        CancellationToken cancellation = default)
        => _application.InvokeRequestAsync(request, requestor, cancellation);

    public ValueTask DisposeAsync()
    {
        _disposing = true;
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _disposing = true;
    }
}