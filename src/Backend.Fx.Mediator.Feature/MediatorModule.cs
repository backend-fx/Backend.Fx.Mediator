using System.Reflection;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature;

public class MediatorModule : IModule
{
    private readonly IMediator _mediator;
    private readonly Assembly[] _assemblies;
    
    public MediatorModule(IMediator mediator, params Assembly[] assemblies)
    {
        _mediator = mediator;
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        var handlerRegistry = new HandlerRegistry();
        
        var notificationHandlerServices = _assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsImplementationOfOpenGenericInterface(typeof(INotificationHandler<>)))
            .Select(type => new ServiceDescriptor(type, type, ServiceLifetime.Scoped));

        foreach (var notificationHandlerService in notificationHandlerServices)
        {
            var notificationType = notificationHandlerService.ImplementationType!.GetTypeInfo()
                .ImplementedInterfaces
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .GenericTypeArguments
                .First();

            var key = new HandlerKey(notificationType);
            handlerRegistry.Add(key, notificationHandlerService.ServiceType);
            compositionRoot.Register(notificationHandlerService);
        }
        
        var requestHandlerServices = _assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsImplementationOfOpenGenericInterface(typeof(IRequestHandler<,>)))
            .Select(type => new ServiceDescriptor(type, type, ServiceLifetime.Scoped));

        foreach (var requestHandlerService in requestHandlerServices)
        {
            Type[] genericTypeArgs = requestHandlerService.ImplementationType!.GetTypeInfo()
                .ImplementedInterfaces
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .GenericTypeArguments
                .ToArray();

            var key = new HandlerKey(genericTypeArgs[0], genericTypeArgs[1]);
            handlerRegistry.Add(key, requestHandlerService.ServiceType);
            compositionRoot.Register(requestHandlerService);
        }
        
        compositionRoot.Register(ServiceDescriptor.Singleton<IHandlerRegistry>(handlerRegistry));
        compositionRoot.Register(ServiceDescriptor.Singleton(_mediator));
    }
}
