using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyNotificationHandler1 : INotificationHandler<MyTestNotification>
{
    private readonly Spy _spy;

    public MyNotificationHandler1(Spy spy)
    {
        _spy = spy;
    }

    public ValueTask HandleAsync(MyTestNotification notification, CancellationToken cancellation = default)
        => _spy.NotificationHandler.HandleAsync(notification, cancellation);
}