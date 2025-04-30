using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;
using Backend.Fx.Mediator.Feature.Internal;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator.Feature;

/// <summary>
/// Provides a scoped <see cref="IMediator"/> instance that can be used to call <see cref="IRequestHandler{TRequest}"/>s
/// synchronously, or to send out notifications to <see cref="INotificationHandler{TNotification}"/>s. Notifications
/// are queued in an outbox and are not published until the surrounding
/// <see cref="Backend.Fx.Execution.Pipeline.IOperation"/> completes.
/// </summary>
[PublicAPI]
public class MediatorFeature : IFeature
{
    private readonly MediatorOptions _options = new();

    public MediatorFeature(Action<MediatorOptions>? configure = null)
    {
        configure?.Invoke(_options);
    }

    public void Enable(IBackendFxApplication application)
    {
        RootMediator = new RootMediator(new Internal.Mediator(application, _options));
        var mediatorModule = new MediatorModule(RootMediator, application.Assemblies);
        application.CompositionRoot.RegisterModules(mediatorModule);
    }

    public IRootMediator RootMediator { get; private set; } = null!;
}