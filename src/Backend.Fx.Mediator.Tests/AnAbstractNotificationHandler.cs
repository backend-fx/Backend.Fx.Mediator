using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;

namespace Backend.Fx.Mediator.Tests;

public abstract class AnAbstractNotificationHandler<TNotification> : INotificationHandler<TNotification>
{
    private readonly INotificationHandler<TNotification> _spy;

    protected AnAbstractNotificationHandler(INotificationHandler<TNotification> spy)
    {
        _spy = spy;
    }

    public ValueTask HandleAsync(TNotification notification, CancellationToken cancellation = default)
    {
        return _spy.HandleAsync(notification, cancellation);
    }
}

public class TheConcreteNotificationHandler3() : AnAbstractNotificationHandler<MyTestNotification3>(Spy)
{
    public static INotificationHandler<MyTestNotification3> Spy { get; } = A.Fake<INotificationHandler<MyTestNotification3>>();
}

public class TheConcreteNotificationHandler2() : AnAbstractNotificationHandler<MyTestNotification2>(Spy)
{
    public static INotificationHandler<MyTestNotification2> Spy { get; } = A.Fake<INotificationHandler<MyTestNotification2>>();
}