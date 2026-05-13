using System.Reflection;
using System.Security.Principal;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution;
using Backend.Fx.Logging;
using Backend.Fx.Mediator.Feature.Diagnostics;
using Backend.Fx.Mediator.Feature.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Backend.Fx.Mediator.Feature;

internal interface IApplicationMediator
{
    Task NotifyAsync<TNotification>(
        TNotification notification,
        IIdentity? notifier = null,
        INotificationErrorHandler? errorHandler = null,
        CancellationToken cancellation = default) where TNotification : class;

    Task<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity? requestor = null,
        CancellationToken cancellation = default) where TResponse : class;
    
}

internal class ApplicationMediator : IApplicationMediator
{
    private readonly ILogger _logger = Log.Create<ApplicationMediator>();
    private readonly IBackendFxApplication _application;
    private readonly HandlerRegistry _handlerRegistry;
    private readonly MediatorOptions _options;
    
    internal ApplicationMediator(
        IBackendFxApplication application,
        HandlerRegistry handlerRegistry,
        MediatorOptions options)
    {
        _application = application;
        _handlerRegistry = handlerRegistry;
        _options = options;
    }

    public Task NotifyAsync<TNotification>(
        TNotification notification,
        IIdentity? notifier = null,
        INotificationErrorHandler? errorHandler = null,
        CancellationToken cancellation = default) where TNotification : class
    {
        notifier ??= _options.DefaultNotifier;
        errorHandler ??= _options.ErrorHandler;

        var notificationHandlerTypes = _handlerRegistry.GetNotificationHandlerTypes<TNotification>();
        if (notificationHandlerTypes.Length == 0)
        {
            _logger.LogInformation("No handler types for {@NotificationType} found.", typeof(TNotification));
            return Task.CompletedTask;
        }

        var tasks = notificationHandlerTypes
            .Select(nht => NotifyHandlerAndHandleError(notification, nht, notifier, errorHandler, cancellation));

        return Task.WhenAll(tasks);
    }

    private async Task NotifyHandlerAndHandleError<TNotification>(
        TNotification notification,
        Type handlerType,
        IIdentity notifier,
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation) where TNotification : notnull
    {
        try
        {
            await _application.Invoker.InvokeAsync(async (sp, ct) =>
            {
                var handler = sp.GetRequiredService(handlerType);

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (handler is IInitializableHandler initializableHandler)
                {
                    await initializableHandler.InitializeAsync(ct).ConfigureAwait(false);
                }

                await ((INotificationHandler<TNotification>)handler).HandleAsync(notification, ct);
            }, notifier, cancellation);
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(handlerType, notification, notifier, ex);
            _application.CompositionRoot.ServiceProvider.GetRequiredService<IFailedNotifications>().Add(
                new FailedNotification(
                    SystemClock.Instance.GetCurrentInstant(), 
                    notification,
                    notifier,
                    ex));
        }
    }

    public async Task<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity? requestor = null,
        CancellationToken cancellation = default) where TResponse : class
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var requestHandlerType = _handlerRegistry.GetRequestHandlerType<TResponse>(requestType);
        var expectedGenericInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        if (!expectedGenericInterfaceType.IsAssignableFrom(requestHandlerType))
        {
            throw new InvalidOperationException(
                $"Handler {requestHandlerType.Name} is not implementing IRequestHandler<{requestType}, {responseType.Name}>");
        }

        TResponse response = null!;

        await _application.Invoker.InvokeAsync(async (sp, ct) =>
        {
            requestor ??= _options.DefaultRequestor;
            var handler = sp.GetRequiredService(requestHandlerType);

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (handler is IInitializableHandler initializableHandler)
            {
                await initializableHandler.InitializeAsync(ct).ConfigureAwait(false);
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (handler is IAuthorizedHandler authorizedHandler)
            {
                var isAuthorized = await authorizedHandler.IsAuthorizedAsync(requestor, ct).ConfigureAwait(false);
                if (isAuthorized == false)
                {
                    throw new ForbiddenException("You are not authorized to perform this action");
                }
            }

            var genericAuthorizedHandlerType = typeof(IAuthorizedHandler<>).MakeGenericType(requestType);
            if (genericAuthorizedHandlerType.IsInstanceOfType(handler))
            {
                var methodInfo = genericAuthorizedHandlerType.GetMethod(
                    "IsAuthorizedAsync",
                    BindingFlags.Instance | BindingFlags.Public);

                if (methodInfo == null)
                {
                    throw new InvalidOperationException(
                        $"Handler {requestHandlerType.Name} does not have an IsAuthorizedAsync method");
                }

                var isAuthorized =
                    await (ValueTask<bool>)(methodInfo.Invoke(handler, [requestor, request, cancellation])
                                            ?? ValueTask.FromResult(false));
                if (isAuthorized != true)
                {
                    throw new ForbiddenException("You are not authorized to perform this action");
                }
            }

            var handleAsyncMethod = requestHandlerType.GetMethod("HandleAsync");
            if (handleAsyncMethod == null)
            {
                throw new InvalidOperationException(
                    $"Handler {requestHandlerType.Name} does not have a HandleAsync method");
            }

            try
            {
                var task = (ValueTask<TResponse>)handleAsyncMethod.Invoke(handler, [request, cancellation])!;
                response = await task.ConfigureAwait(false);
            }
            catch (TargetInvocationException tex)
            {
                throw tex.InnerException ?? tex;
            }
        }, requestor, cancellation);

        return response;
    }
}