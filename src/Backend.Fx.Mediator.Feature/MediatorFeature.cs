using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;

namespace Backend.Fx.Mediator.Feature;

public class MediatorFeature : IFeature
{
    private readonly MediatorOptions _options = new();
    
    public MediatorFeature(Action<MediatorOptions>? configure = null)
    {
        configure?.Invoke(_options);
    }
    
    public void Enable(IBackendFxApplication application)
    {
        var mediatorModule = new MediatorModule(new Mediator(application, _options), application.Assemblies);
        application.CompositionRoot.RegisterModules(mediatorModule);
    }
}