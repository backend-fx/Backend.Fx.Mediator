using Backend.Fx.Execution;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Mediator.Feature.AutoNotification;
using Backend.Fx.Mediator.Feature.Outbox;
using Backend.Fx.Mediator.Feature.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature;

internal class MediatorModule : IModule
{
    private readonly IBackendFxApplication _application;
    private readonly MediatorOptions _options;

    public HandlerRegistry HandlerRegistry { get; private set; } = null!;

    internal MediatorModule(IBackendFxApplication application, MediatorOptions options)
    {
        _application = application;
        _options = options;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        // scan assemblies for request and notification handlers and register them
        HandlerRegistry = new HandlerRegistry(_application.Assemblies);
        foreach (var handlerType in HandlerRegistry)
        {
            compositionRoot.Register(new ServiceDescriptor(handlerType, handlerType, ServiceLifetime.Scoped));
        }

        // register the application mediator as singleton
        compositionRoot.Register(
            ServiceDescriptor.Singleton<IApplicationMediator>(
                new ApplicationMediator(_application, HandlerRegistry, _options)));

        // register the user code facing, scoped mediator (with optional addon decorators)
        compositionRoot.Register(ServiceDescriptor.Scoped<IMediator, Mediator>());

        if (_options.UseOutbox)
        {
            compositionRoot.Register(ServiceDescriptor.Scoped<IMediatorOutbox, MediatorOutbox>());
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IMediator, WithOutbox>());
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, FlushMediatorOutboxOperation>());
        }

        if (_options.AutoNotifyResponses)
        {
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IMediator, WithAutoNotification>());
        }
    }
}