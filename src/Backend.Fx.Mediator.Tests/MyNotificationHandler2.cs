using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyNotificationHandler2 : INotificationHandler<MyTestNotification>
{
    private readonly Spy _spy;

    public MyNotificationHandler2(Spy spy)
    {
        _spy = spy;
    }

    public ValueTask HandleAsync(MyTestNotification notification, CancellationToken cancellation = default)
        => _spy.NotificationHandler.HandleAsync(notification, cancellation);
}