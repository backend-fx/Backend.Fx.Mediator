using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using Backend.Fx.Execution;
using Backend.Fx.Logging;
using Backend.Fx.Mediator.Feature.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Mediator.Feature.MediatorImplementation;

internal class RootMediator : IRootMediator
{
    private readonly ILogger _logger = Log.Create<RootMediator>();
    private readonly IBackendFxApplication _application;
    private readonly MediatorOptions _options;
    private readonly HandlerRegistry _handlerRegistry;

    internal RootMediator(IBackendFxApplication application, MediatorOptions options, HandlerRegistry handlerRegistry)
    {
        _application = application;
        _options = options;
        _handlerRegistry = handlerRegistry;
    }

    [SuppressMessage(
        "Usage",
        "CA2012:Use ValueTasks correctly", 
        Justification = "This method allows processing of notifications in the background. Await it only if you need to wait for all notifications to be processed.")]
    public ValueTask NotifyAsync<TNotification>(
        TNotification notification,
        IIdentity notifier,
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation) where TNotification : class
    {
        var notificationHandlerTypes = _handlerRegistry.GetNotificationHandlerTypes<TNotification>();
        if (notificationHandlerTypes.Length == 0)
        {
            _logger.LogInformation("No handler types for {@NotificationType} found.", typeof(TNotification));
            return ValueTask.CompletedTask;
        }

        var tasks = new List<Task>();

        foreach (var notificationHandlerType in notificationHandlerTypes)
        {
            var task = _application.Invoker.InvokeAsync(async (sp, ct) =>
            {
                try
                {
                    var handler = sp.GetRequiredService(notificationHandlerType);

                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if (handler is IInitializableHandler initializableHandler)
                    {
                        await initializableHandler.InitializeAsync(ct).ConfigureAwait(false);
                    }

                    await ((INotificationHandler<TNotification>)handler).HandleAsync(notification, ct);
                }
                catch (Exception ex)
                {
                    errorHandler.HandleError(notificationHandlerType, notification, notifier, ex);
                }
            }, notifier, cancellation);

            tasks.Add(task);
        }

        return new ValueTask(Task.WhenAll(tasks));
    }


    public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellation = default)
        where TNotification : class
        => NotifyAsync(notification, _options.DefaultNotifier, _options.ErrorHandler, cancellation);

    public ValueTask NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        CancellationToken cancellation = default) where TNotification : class
        => NotifyAsync(notification, notifier, _options.ErrorHandler, cancellation);

    public ValueTask NotifyAsync<TNotification>(TNotification notification, INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default) where TNotification : class
        => NotifyAsync(notification, _options.DefaultNotifier, errorHandler, cancellation);

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class
        => RequestAsync(request, _options.DefaultRequestor, cancellation);

    public async ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class
    {
        TResponse response = null!;

        await _application.Invoker.InvokeAsync(
            async (sp, ct) =>
                response = await new RequestDispatch(_handlerRegistry, sp).DispatchAsync(request, requestor, ct),
            requestor,
            cancellation).ConfigureAwait(false);

        if (_options.AutoNotifyResponses)
        {
            _logger.LogInformation("Sending response of type {Response} also as notification", typeof(TResponse).Name);
            await NotifyAsync(response, cancellation).ConfigureAwait(false);
        }

        return response;
    }
}