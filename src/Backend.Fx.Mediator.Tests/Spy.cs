using FakeItEasy;

namespace Backend.Fx.Mediator.Tests;

public class Spy
{
    public IRequestHandler<MyTestRequest, string> RequestHandler { get; } =
        A.Fake<IRequestHandler<MyTestRequest, string>>();

    public INotificationHandler<MyTestNotification> NotificationHandler { get; } =
        A.Fake<INotificationHandler<MyTestNotification>>();
}