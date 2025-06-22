using System.Security.Principal;
using Backend.Fx.Logging;
using Backend.Fx.Mediator.Feature.Registry;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Mediator.Feature.MediatorImplementation;

internal sealed class Mediator : IMediator
{
    private readonly ILogger _logger = Log.Create<Mediator>();
    private readonly IRootMediator _rootMediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly MediatorOptions _options;
    private readonly HandlerRegistry _handlerRegistry;

    public Mediator(IRootMediator rootMediator, IServiceProvider serviceProvider, MediatorOptions options, HandlerRegistry handlerRegistry)
    {
        _rootMediator = rootMediator;
        _serviceProvider = serviceProvider;
        _options = options;
        _handlerRegistry = handlerRegistry;
    }


    public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, notifier, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, errorHandler, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        INotificationErrorHandler errorHandler, CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, notifier, errorHandler, cancellation);
    }

    public ValueTask<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default) where TResponse : class
    {
        return _rootMediator.RequestAsync(request, cancellation);
    }

    public async ValueTask<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, IIdentity requestor, CancellationToken cancellation = default) where TResponse : class
    {
        TResponse response = await new RequestDispatch(_handlerRegistry, _serviceProvider)
            .DispatchAsync(request, requestor, cancellation)
            .ConfigureAwait(false);
        
        if (_options.AutoNotifyResponses)
        {
            _logger.LogInformation("Sending response of type {Response} also as notification", typeof(TResponse).Name);
            await NotifyAsync(response, cancellation).ConfigureAwait(false);
        }

        return response;
    }
}