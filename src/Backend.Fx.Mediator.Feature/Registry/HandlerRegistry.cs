using System.Collections;
using System.Reflection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature.Registry;

internal class HandlerRegistry : IEnumerable<Type>
{
    private readonly ILookup<HandlerKey, Type> _handlerTypeLookup;

    public HandlerRegistry(IEnumerable<Assembly> assemblies)
    {
        assemblies = assemblies as Assembly[] ?? assemblies.ToArray();
        
        List<(HandlerKey handlerKey, Type handlerType)> handlers = [];
        
        var notificationHandlerServiceDescriptors = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsInterface && type.IsClass && !type.IsAbstract)
            .Where(type => type.IsImplementationOfOpenGenericInterface(typeof(INotificationHandler<>)))
            .Select(type => new ServiceDescriptor(type, type, ServiceLifetime.Scoped));

        foreach (var notificationHandlerServiceDescriptor in notificationHandlerServiceDescriptors)
        {
            var notificationType = notificationHandlerServiceDescriptor.ImplementationType!.GetTypeInfo()
                .ImplementedInterfaces
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .GenericTypeArguments
                .First();

            var key = new HandlerKey(notificationType);
            handlers.Add((key, notificationHandlerServiceDescriptor.ServiceType));
        }
        
        var requestHandlerServiceDescriptors = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsInterface && type.IsClass && !type.IsAbstract)
            .Where(type => type.IsImplementationOfOpenGenericInterface(typeof(IRequestHandler<,>)))
            .Select(type => new ServiceDescriptor(type, type, ServiceLifetime.Scoped));

        foreach (var requestHandlerServiceDescriptor in requestHandlerServiceDescriptors)
        {
            Type[] genericTypeArgs = requestHandlerServiceDescriptor.ImplementationType!.GetTypeInfo()
                .ImplementedInterfaces
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .GenericTypeArguments
                .ToArray();

            var key = new HandlerKey(genericTypeArgs[0], genericTypeArgs[1]);
            handlers.Add((key, requestHandlerServiceDescriptor.ServiceType));
        }

        _handlerTypeLookup = handlers.ToLookup(tuple => tuple.handlerKey, tuple => tuple.handlerType);
    }
    
    public Type GetRequestHandlerType<TResponse>(Type requestType)
    {
        var key = new HandlerKey(requestType, typeof(TResponse));
        var handlerTypes = _handlerTypeLookup[key].ToArray();
        return handlerTypes.SingleOrDefault()
               ?? throw new InvalidOperationException(
                   $"No handler found for request type {requestType.Name} with response type {typeof(TResponse).Name}");
    }

    public Type[] GetNotificationHandlerTypes<TNotification>() where TNotification : class
    {
        var key = HandlerKey.For<TNotification>();
        var handlerTypes = _handlerTypeLookup[key].ToArray();
        return handlerTypes;
    }

    public IEnumerator<Type> GetEnumerator()
    {
        return _handlerTypeLookup.SelectMany(grouping => grouping).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _handlerTypeLookup.SelectMany(grouping => grouping).GetEnumerator();
    }

    public HandlerMetaData[] GetMetaData()
    {
        return _handlerTypeLookup
            .SelectMany(types => types.Select(type => new HandlerMetaData(types.Key, type)))
            .ToArray();
    }
}