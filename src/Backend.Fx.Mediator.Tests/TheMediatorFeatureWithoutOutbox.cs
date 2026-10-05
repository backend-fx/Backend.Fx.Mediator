using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Backend.Fx.Mediator.Feature;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Backend.Fx.Mediator.Tests;

public class TheMediatorFeatureWithoutOutbox : IAsyncLifetime
{
    private readonly MyInitializedRequestSpy _initializedRequestSpy = new();
    private readonly MyTestRequestSpy _testRequestSpy = new();
    private readonly MyAuthorizedRequestSpy _authorizedRequestSpy = new();
    private readonly MyTestNotificationSpy _testNotificationSpy = new();
    private readonly BackendFxApplication _application;

    public TheMediatorFeatureWithoutOutbox()
    {
        _application = new BackendFxApplication(
            new SimpleInjectorCompositionRoot(),
            A.Fake<IExceptionLogger>(),
            GetType().Assembly
        );

        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_authorizedRequestSpy));
        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_testRequestSpy));
        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_testNotificationSpy));
        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_initializedRequestSpy));

        _application.EnableFeature(
            new MediatorFeature(opt =>
            {
                opt.UseOutbox = false;
                opt.AutoNotifyResponses = true;
            })
        );
    }

    public async ValueTask InitializeAsync() => await _application.BootAsync();

    [Fact]
    public async Task AwaitsAutoNotificationBeforeReturningTheResponse()
    {
        await _application.RequestAsync(
            new MyAutoNotifiedRequest(),
            cancellation: TestContext.Current.CancellationToken
        );

        MyAutoNotifiedResponseHandler.HasCompleted.ShouldBeTrue();
    }

    public async ValueTask DisposeAsync() => await _application.DisposeAsync();
}
