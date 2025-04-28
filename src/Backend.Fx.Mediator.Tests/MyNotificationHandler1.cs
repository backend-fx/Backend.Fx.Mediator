using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyNotificationHandler1 : INotificationHandler<MyTestNotification>
{
    private readonly MyTestNotificationSpy _spy;

    public MyNotificationHandler1(MyTestNotificationSpy spy)
    {
        _spy = spy;
    }

    public ValueTask HandleAsync(MyTestNotification notification, CancellationToken cancellation = default)
        => _spy.NotificationHandler.HandleAsync(notification, cancellation);
}