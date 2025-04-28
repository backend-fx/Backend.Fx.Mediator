using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;

namespace Backend.Fx.Mediator.Tests;

public class MyTestRequestSpy
{
    public IRequestHandler<MyTestRequest, string> RequestHandler { get; } =
        A.Fake<IRequestHandler<MyTestRequest, string>>();
}

public class MyAuthorizedRequestSpy
{
    public IAuthorizedHandler AuthorizedHandler { get; } =
        A.Fake<IAuthorizedHandler>();

    public Task<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation)
    {
        return AuthorizedHandler.IsAuthorizedAsync(identity, cancellation);
    }
}

public class MyTestNotificationSpy
{
    public INotificationHandler<MyTestNotification> NotificationHandler { get; } =
        A.Fake<INotificationHandler<MyTestNotification>>();
}

public class MyInitializedRequestSpy
{
    public IInitializableHandler InitializableHandler { get; } =
        A.Fake<IInitializableHandler>();

    public ValueTask InitializeAsync(CancellationToken cancellation)
    {
        return InitializableHandler.InitializeAsync(cancellation);
    }
}