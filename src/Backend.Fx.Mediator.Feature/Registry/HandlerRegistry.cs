using System.Collections;
using System.Reflection;

namespace Backend.Fx.Mediator.Feature.Registry;

internal class HandlerRegistry : IEnumerable<Type>
{
    private readonly ILookup<HandlerKey, Type> _handlerTypeLookup;

    public HandlerRegistry(IEnumerable<Assembly> assemblies)
    {
        List<(HandlerKey handlerKey, Type handlerType)> handlers = [];

        var candidateTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsInterface && type.IsClass && !type.IsAbstract);

        foreach (var candidateType in candidateTypes)
        {
            var implementedInterfaces = candidateType.GetTypeInfo()
                .ImplementedInterfaces
                .Where(i => i.IsGenericType)
                .ToArray();

            foreach (var notificationInterface in implementedInterfaces
                         .Where(i => i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
            {
                var key = new HandlerKey(notificationInterface.GenericTypeArguments[0]);
                handlers.Add((key, candidateType));
            }

            var requestInterfaces = implementedInterfaces
                .Where(i => i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .ToArray();

            foreach (var requestInterface in requestInterfaces)
            {
                var key = new HandlerKey(
                    requestInterface.GenericTypeArguments[0],
                    requestInterface.GenericTypeArguments[1]);
                handlers.Add((key, candidateType));
            }

            // handlers of requests responding with a SuccessResponse may implement the single arg interface
            foreach (var successRequestInterface in implementedInterfaces
                         .Where(i => i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
            {
                var requestType = successRequestInterface.GenericTypeArguments[0];
                var isAlsoImplementingTheTwoArgInterface = requestInterfaces.Any(
                    ri => ri.GenericTypeArguments[0] == requestType
                          && ri.GenericTypeArguments[1] == typeof(SuccessResponse));

                if (isAlsoImplementingTheTwoArgInterface)
                {
                    continue;
                }

                var key = new HandlerKey(requestType, typeof(SuccessResponse));
                handlers.Add((key, candidateType));
            }
        }

        _handlerTypeLookup = handlers.ToLookup(tuple => tuple.handlerKey, tuple => tuple.handlerType);
    }
    
    public Type GetRequestHandlerType<TResponse>(Type requestType)
    {
        var key = new HandlerKey(requestType, typeof(TResponse));
        var handlerTypes = _handlerTypeLookup[key].ToArray();

        return handlerTypes.Length switch
        {
            1 => handlerTypes[0],
            0 => throw new InvalidOperationException(
                $"No handler found for request type {requestType.Name} with response type {typeof(TResponse).Name}"),
            _ => throw new InvalidOperationException(
                $"Multiple handlers found for request type {requestType.Name} with response type "
                + $"{typeof(TResponse).Name}: {string.Join(", ", handlerTypes.Select(t => t.Name))}. "
                + "A request must be handled by exactly one handler.")
        };
    }

    public Type[] GetNotificationHandlerTypes<TNotification>() where TNotification : class
    {
        var key = HandlerKey.For<TNotification>();
        var handlerTypes = _handlerTypeLookup[key].ToArray();
        return handlerTypes;
    }

    public IEnumerator<Type> GetEnumerator()
    {
        // a handler type might be registered for multiple keys, but must be enumerated (and registered) only once
        return _handlerTypeLookup.SelectMany(grouping => grouping).Distinct().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public HandlerMetaData[] GetMetaData()
    {
        return _handlerTypeLookup
            .SelectMany(types => types.Select(type => new HandlerMetaData(types.Key, type)))
            .ToArray();
    }
}