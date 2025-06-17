using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyNotificationHandler1 : INotificationHandler<MyTestNotification1>
{
    private readonly MyTestNotificationSpy _spy;

    public MyNotificationHandler1(MyTestNotificationSpy spy)
    {
        _spy = spy;
    }

    public ValueTask HandleAsync(MyTestNotification1 notification1, CancellationToken cancellation = default)
        => _spy.NotificationHandler.HandleAsync(notification1, cancellation);
}