using System.Reflection;
using Backend.Fx.Execution;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Mediator.Feature.MediatorImplementation;
using Backend.Fx.Mediator.Feature.Outbox;
using Backend.Fx.Mediator.Feature.Registry;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature;

internal class MediatorModule : IModule
{
    private readonly IBackendFxApplication _application;
    private readonly MediatorOptions _options;
    private readonly HandlerRegistry _handlerRegistry = new();
    
    internal MediatorModule(IBackendFxApplication application, MediatorOptions options)
    {
        _application = application;
        _options = options;
    }

    public IEnumerable<HandlerMetaData> Handlers => _handlerRegistry.MetaData;
    
    public void Register(ICompositionRoot compositionRoot)
    {
        var notificationHandlerServices = _application.Assemblies
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
            _handlerRegistry.Add(key, notificationHandlerService.ServiceType);
            compositionRoot.Register(notificationHandlerService);
        }
        
        var requestHandlerServices = _application.Assemblies
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
            _handlerRegistry.Add(key, requestHandlerService.ServiceType);
            compositionRoot.Register(requestHandlerService);
        }

        var rootMediator = new RootMediator(_application, _options, _handlerRegistry);
        compositionRoot.Register(ServiceDescriptor.Singleton<IRootMediator>(rootMediator));

        compositionRoot.Register(ServiceDescriptor.Scoped<IMediator, MediatorImplementation.Mediator>());
        compositionRoot.Register(ServiceDescriptor.Scoped<IMediatorOutbox, MediatorOutbox>());
        compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IMediator, WithOutbox>());
        compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, FlushMediatorOutboxOperation>());

    }
}
