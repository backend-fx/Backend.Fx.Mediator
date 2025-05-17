using System.Reflection;
using System.Security.Principal;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Mediator.Feature.Internal;

[PublicAPI]
public static class BackendFxApplicationMediatorExtensions
{
    private static readonly ILogger Logger = Log.Create(typeof(BackendFxApplicationMediatorExtensions));

    public static async ValueTask<TResponse> InvokeRequestAsync<TResponse>(
        this IBackendFxApplication application,
        IRequest<TResponse> request,
        IIdentity identity,
        CancellationToken cancellation = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var requestHandlerType = application.GetRequestHandlerType<TResponse>(requestType);

        var expectedGenericInterfaceType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);

        if (expectedGenericInterfaceType.IsAssignableFrom(requestHandlerType))
        {
            TResponse? response = default;
            await application.Invoker.InvokeAsync(async (sp, ct) =>
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
                    var isAuthorized = await authorizedHandler.IsAuthorizedAsync(identity, ct).ConfigureAwait(false);
                    if (isAuthorized == false)
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
            }, identity, cancellation).ConfigureAwait(false);

            return response!;
        }

        throw new InvalidOperationException(
            $"Handler {requestHandlerType.Name} is not implementing IRequestHandler<{requestType}, {responseType.Name}>");
    }

    private static Type GetRequestHandlerType<TResponse>(this IBackendFxApplication application, Type requestType)
    {
        var registry = application.CompositionRoot.ServiceProvider.GetRequiredService<IHandlerRegistry>();
        var key = new HandlerKey(requestType, typeof(TResponse));
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