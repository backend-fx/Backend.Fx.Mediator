using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution;
using Backend.Fx.Logging;
using Backend.Fx.Mediator.Feature.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static System.Threading.Tasks.ValueTask;

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

    public async ValueTask NotifyAsync<TNotification>(
        TNotification notification,
        IIdentity notifier,
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation) where TNotification : class
    {
        var notificationHandlerTypes = GetNotificationHandlerTypes<TNotification>();
        if (notificationHandlerTypes.Length == 0)
        {
            _logger.LogInformation("No handler types for {@NotificationType} found.", typeof(TNotification));
            return;
        }

        var tasks = new ConcurrentBag<Task>();
        notificationHandlerTypes.AsParallel().ForAll(notificationHandlerType =>
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await _application.Invoker.InvokeAsync(async (sp, ct) =>
                    {
                        var handler =
                            (INotificationHandler<TNotification>)sp.GetRequiredService(notificationHandlerType);

                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if (handler is IInitializableHandler initializableHandler)
                        {
                            await initializableHandler.InitializeAsync(ct).ConfigureAwait(false);
                        }

                        await handler.HandleAsync(notification, ct);
                    }, notifier, cancellation).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errorHandler.HandleError(notificationHandlerType, notification, notifier, ex);
                }
            }, cancellation));
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
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
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var requestHandlerType = GetRequestHandlerType<TResponse>(requestType);

        var expectedGenericInterfaceType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);

        if (expectedGenericInterfaceType.IsAssignableFrom(requestHandlerType))
        {
            TResponse? response = null;
            await _application.Invoker.InvokeAsync(async (sp, ct) =>
            {
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

                    var isAuthorized = await (ValueTask<bool>)(methodInfo.Invoke(handler, [requestor, request, ct]) ?? FromResult(false));
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
                    var task = (ValueTask<TResponse>)handleAsyncMethod.Invoke(handler, [request, ct])!;
                    response = await task.ConfigureAwait(false);
                }
                catch (TargetInvocationException tex)
                {
                    throw tex.InnerException ?? tex;
                }
            }, requestor, cancellation).ConfigureAwait(false);

            return response!;
        }

        throw new InvalidOperationException(
            $"Handler {requestHandlerType.Name} is not implementing IRequestHandler<{requestType}, {responseType.Name}>");
    }


    private Type GetRequestHandlerType<TResponse>(Type requestType)
    {
        var key = new HandlerKey(requestType, typeof(TResponse));
        var handlerTypes = _handlerRegistry.GetHandlerTypes(key).ToArray();
        return handlerTypes.Single();
    }

    private Type[] GetNotificationHandlerTypes<TNotification>() where TNotification : class
    {
        var key = HandlerKey.For<TNotification>();
        var handlerTypes = _handlerRegistry.GetHandlerTypes(key).ToArray();
        return handlerTypes;
    }
}