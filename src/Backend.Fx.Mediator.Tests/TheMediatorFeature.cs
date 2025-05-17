using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Backend.Fx.Mediator.Feature;
using Backend.Fx.Mediator.Feature.Outbox;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.Mediator.Tests;

public class TheMediatorFeature : IAsyncLifetime
{
    private readonly MyInitializedRequestSpy _initializedRequestSpy = new();
    private readonly MyTestRequestSpy _testRequestSpy = new();
    private readonly MyAuthorizedRequestSpy _authorizedRequestSpy = new();
    private readonly MyTestNotificationSpy _testNotificationSpy = new();
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly INotificationErrorHandler _errorHandler = A.Fake<INotificationErrorHandler>();
    private readonly BackendFxApplication _application;

    public TheMediatorFeature()
    {
        _application = new BackendFxApplication(
            new SimpleInjectorCompositionRoot(),
            _exceptionLogger,
            GetType().Assembly);

        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_authorizedRequestSpy));
        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_testRequestSpy));
        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_testNotificationSpy));
        _application.CompositionRoot.Register(ServiceDescriptor.Singleton(_initializedRequestSpy));

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
            mediator.ShouldBeOfType<WithOutbox>();
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CallsTheSingleRequestHandler()
    {
        await _application.RequestAsync(new MyTestRequest());

        A.CallTo(() => _testRequestSpy.RequestHandler.HandleAsync(A<MyTestRequest>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CallsAllNotificationHandlers()
    {
        await _application.NotifyAsync(new MyTestNotification());

        A.CallTo(() =>
                _testNotificationSpy.NotificationHandler.HandleAsync(A<MyTestNotification>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task FailingRequestIsPropagated()
    {
        await Assert.ThrowsAsync<DivideByZeroException>(async () =>
            await _application.RequestAsync(new FailingRequest()));
    }

    [Fact]
    public async Task FailingNotificationIsHandled()
    {
        await _application.NotifyAsync(new FailingNotification());

        A.CallTo(() =>
                _errorHandler.HandleError(A<Type>._, A<FailingNotification>._, A<IIdentity>._,
                    A<DivideByZeroException>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CallsIsAuthorizedOnAuthorizedHandler()
    {
        A.CallTo(() =>
                _authorizedRequestSpy.AuthorizedHandler.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._))
            .Returns(true);

        await _application.RequestAsync(new MyAuthorizedRequest());

        A.CallTo(() =>
                _authorizedRequestSpy.AuthorizedHandler.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CallsInitializeOnInitializableHandler()
    {
        await _application.RequestAsync(new MyInitializedRequest());

        A.CallTo(() =>
                _initializedRequestSpy.InitializableHandler.InitializeAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task IsAuthorizedThrowsForbiddenException()
    {
        A.CallTo(() =>
                _authorizedRequestSpy.AuthorizedHandler.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._))
            .Returns(false);

        await Assert.ThrowsAsync<ForbiddenException>(async () =>
            await _application.RequestAsync(new MyAuthorizedRequest()));
    }

    public Task DisposeAsync()
    {
        _application.Dispose();
        return Task.CompletedTask;
    }
}