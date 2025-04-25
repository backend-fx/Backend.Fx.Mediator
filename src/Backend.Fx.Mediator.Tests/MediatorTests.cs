using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Backend.Fx.Mediator.Feature;
using Backend.Fx.Mediator.Feature.Internal;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.Mediator.Tests;

public class TheMediatorFeature : IAsyncLifetime
{
    private readonly Spy _spy = new();
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly INotificationErrorHandler _errorHandler = A.Fake<INotificationErrorHandler>();
    private readonly BackendFxApplication _application;

    public TheMediatorFeature()
    {
        _application = new BackendFxApplication(
            new SimpleInjectorCompositionRoot(),
            _exceptionLogger,
            GetType().Assembly);

        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_spy));

        _application.EnableFeature(new MediatorFeature(opt =>
        {
            opt.DefaultRequestor = new GenericIdentity("TestUser");
            opt.ErrorHandler = _errorHandler;
        }));
    }

    public Task InitializeAsync() => _application.BootAsync();

    [Fact]
    public async Task InjectedMediatorInstanceIsScopedOutbox()
    {
        Assert.Throws<ActivationException>(() =>
            _application.CompositionRoot.ServiceProvider.GetRequiredService<IMediator>());

        await _application.Invoker.InvokeAsync((sp, _) =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            mediator.ShouldBeOfType<MediatorOutbox>();
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CallsTheSingleRequestHandler()
    {
        await _application.Invoker.InvokeAsync(async (sp, ct) =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            await mediator.RequestAsync(new MyTestRequest(), ct);
        });

        A.CallTo(() => _spy.RequestHandler.HandleAsync(A<MyTestRequest>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CallsAllNotificationHandlers()
    {
        await _application.Invoker.InvokeAsync((sp, ct) =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            mediator.Notify(new MyTestNotification(), ct);
            return Task.CompletedTask;
        });

        A.CallTo(() => _spy.NotificationHandler.HandleAsync(A<MyTestNotification>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }
    
    [Fact]
    public async Task FailingRequestIsPropagated()
    {
        await Assert.ThrowsAsync<DivideByZeroException>(async () =>
            await _application.Invoker.InvokeAsync(async (sp, ct) =>
            {
                var mediator = sp.GetRequiredService<IMediator>();
                await mediator.RequestAsync(new FailingRequest(), ct);

                throw new Exception("we should not get here, because the request should fail with DivideByZeroException");
            }));
    }

    [Fact]
    public async Task FailingNotificationIsHandled()
    {
        await _application.Invoker.InvokeAsync((sp, ct) =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            mediator.Notify(new FailingNotification(), ct);
            return Task.CompletedTask;
        });

        A.CallTo(() => _errorHandler.HandleError(A<Type>._, A<FailingNotification>._, A<IIdentity>._, A<DivideByZeroException>._))
            .MustHaveHappenedOnceExactly();
    }

    public Task DisposeAsync()
    {
        _application.Dispose();
        return Task.CompletedTask;
    }
}