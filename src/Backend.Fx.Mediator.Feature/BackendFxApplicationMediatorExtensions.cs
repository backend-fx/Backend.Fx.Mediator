using System.Security.Principal;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Mediator.Feature;

[PublicAPI]
public static class BackendFxApplicationMediatorExtensions
{
    private static readonly ILogger Logger = Log.Create(typeof(BackendFxApplicationMediatorExtensions));

    public static async ValueTask<TResponse> InvokeRequestAsync<TRequest, TResponse>(
        this IBackendFxApplication application,
        TRequest request,
        IIdentity identity,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
    {
        var requestHandlerType = application.GetRequestHandlerType<TRequest, TResponse>();

        if (requestHandlerType.GetInterfaces().Any(ift => ift == typeof(IRequestHandler<TRequest, TResponse>)))
        {
            TResponse? response = default;
            await application.Invoker.InvokeAsync(async (sp, ct) =>
            {
                var handler = (IRequestHandler<TRequest, TResponse>)sp.GetRequiredService(requestHandlerType);

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (handler is IInitializableHandler initializableHandler)
                {
                    await initializableHandler.InitializeAsync(ct).ConfigureAwait(false);
                }

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (handler is IAuthorizedHandler authorizedHandler)
                {
                    var isAuthorized = await authorizedHandler.IsAuthorizedAsync(identity, ct).ConfigureAwait(false);
                    if (isAuthorized == false)
                    {
                        throw new ForbiddenException("You are not authorized to perform this action");
                    }
                }

                response = await handler.HandleAsync(request, ct);
            }, identity, cancellation).ConfigureAwait(false);

            return response ?? throw new InvalidOperationException($"Handler {requestHandlerType.Name} returned null");
        }

        throw new InvalidOperationException(
            $"Handler {requestHandlerType.Name} is not implementing IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}>");
    }

    public static async ValueTask NotifyAsync<TNotification>(
        this IBackendFxApplication application,
        TNotification notification,
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation) where TNotification : class
    {
        var notificationHandlerTypes = application.GetNotificationHandlerTypes<TNotification>();
        if (notificationHandlerTypes.Length == 0)
        {
            Logger.LogInformation("No handler types for {@NotificationType} found.", typeof(TNotification));
            return;
        }

        foreach (var notificationHandlerType in notificationHandlerTypes)
        {
            await application.Invoker.InvokeAsync(async (sp, ct) =>
            {
                var handler = (INotificationHandler<TNotification>)sp.GetRequiredService(notificationHandlerType);

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (handler is IInitializableHandler initializableHandler)
                {
                    await initializableHandler.InitializeAsync(ct).ConfigureAwait(false);
                }

                await handler.HandleAsync(notification, ct);
            }, new SystemIdentity(), cancellation).ConfigureAwait(false);
        }
    }

    private static Type GetRequestHandlerType<TRequest, TResponse>(this IBackendFxApplication application)
        where TRequest : IRequest<TResponse>
    {
        var registry = application.CompositionRoot.ServiceProvider.GetRequiredService<IHandlerRegistry>();
        var key = HandlerKey.For<TRequest, TResponse>();
        var handlerTypes = registry.GetHandlerTypes(key).ToArray();
        return handlerTypes.Single();
    }

    private static Type[] GetNotificationHandlerTypes<TNotification>(this IBackendFxApplication application)
        where TNotification : class
    {
        var registry = application.CompositionRoot.ServiceProvider.GetRequiredService<IHandlerRegistry>();
        var key = HandlerKey.For<TNotification>();
        var handlerTypes = registry.GetHandlerTypes(key).ToArray();
        return handlerTypes;
    }
}