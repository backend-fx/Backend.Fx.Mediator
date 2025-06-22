using System.Reflection;
using System.Security.Principal;
using Backend.Fx.Exceptions;
using Backend.Fx.Mediator.Feature.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature.MediatorImplementation;

internal class RequestDispatch
{
    private readonly HandlerRegistry _handlerRegistry;
    private readonly IServiceProvider _serviceProvider;

    public RequestDispatch(HandlerRegistry handlerRegistry, IServiceProvider serviceProvider)
    {
        _handlerRegistry = handlerRegistry;
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<TResponse> DispatchAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var requestHandlerType = _handlerRegistry.GetRequestHandlerType<TResponse>(requestType);

        var expectedGenericInterfaceType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);

        if (!expectedGenericInterfaceType.IsAssignableFrom(requestHandlerType))
        {
            throw new InvalidOperationException(
                $"Handler {requestHandlerType.Name} is not implementing IRequestHandler<{requestType}, {responseType.Name}>");
        }

        var handler = _serviceProvider.GetRequiredService(requestHandlerType);

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (handler is IInitializableHandler initializableHandler)
        {
            await initializableHandler.InitializeAsync(cancellation).ConfigureAwait(false);
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (handler is IAuthorizedHandler authorizedHandler)
        {
            var isAuthorized = await authorizedHandler.IsAuthorizedAsync(requestor, cancellation).ConfigureAwait(false);
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

            var isAuthorized = await (ValueTask<bool>)(methodInfo.Invoke(handler, [requestor, request, cancellation])
                                                       ?? ValueTask.FromResult(false));
            if (isAuthorized != true)
            {
                throw new ForbiddenException("You are not authorized to perform this action");
            }
        }

        var handleAsyncMethod = requestHandlerType.GetMethod("HandleAsync");
        if (handleAsyncMethod == null)
        {
            throw new InvalidOperationException($"Handler {requestHandlerType.Name} does not have a HandleAsync method");
        }

        try
        {
            var task = (ValueTask<TResponse>)handleAsyncMethod.Invoke(handler, [request, cancellation])!;
            return await task.ConfigureAwait(false);
        }
        catch (TargetInvocationException tex)
        {
            throw tex.InnerException ?? tex;
        }
    }
}