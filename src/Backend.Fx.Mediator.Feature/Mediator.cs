using System.Security.Principal;
using Backend.Fx.Execution;

namespace Backend.Fx.Mediator.Feature;

public class Mediator : IMediator
{
    private readonly IBackendFxApplication _application;
    private readonly MediatorOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private int _runningTaskCount;
    private volatile bool _disposing;

    public Mediator(IBackendFxApplication application, MediatorOptions options)
    {
        _application = application;
        _options = options;
    }

    public void Notify<TNotification>(TNotification notification) where TNotification : class
    {
        // Don't accept new notifications during disposal
        if (_disposing)
        {
            return;
        }
        
        Task.Run(async () =>
        {
            Interlocked.Increment(ref _runningTaskCount);
            try
            {
                await _application.NotifyAsync(notification, _options.ErrorHandler, _cancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _runningTaskCount);
            }
        });
    }

    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        => RequestAsync<TRequest, TResponse>(request, _options.DefaultRequestor, cancellation);

    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, IIdentity requestor,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        => _application.InvokeRequestAsync<TRequest, TResponse>(request, requestor, cancellation);

    public async ValueTask DisposeAsync()
    {
        // Mark as disposing to prevent new task creation
        _disposing = true;
        
        // Graceful exit
        var startTime = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(3);
        while (_runningTaskCount > 0 && DateTime.UtcNow - startTime < timeout)
        {
            await Task.Delay(50).ConfigureAwait(false);
        }

        // Kill remaining tasks
        if (_runningTaskCount > 0)
        {
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        }

        _cancellationTokenSource.Dispose();
    }
}
