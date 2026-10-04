using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;

namespace Backend.Fx.Mediator.Tests;

public record MyTestNotification4;

public record MyTestNotification5;

public class MyMultiNotificationHandler : INotificationHandler<MyTestNotification4>,
    INotificationHandler<MyTestNotification5>
{
    public static INotificationHandler<MyTestNotification4> Spy4 { get; } =
        A.Fake<INotificationHandler<MyTestNotification4>>();

    public static INotificationHandler<MyTestNotification5> Spy5 { get; } =
        A.Fake<INotificationHandler<MyTestNotification5>>();

    public ValueTask HandleAsync(MyTestNotification4 notification, CancellationToken cancellation = default)
        => Spy4.HandleAsync(notification, cancellation);

    public ValueTask HandleAsync(MyTestNotification5 notification, CancellationToken cancellation = default)
        => Spy5.HandleAsync(notification, cancellation);
}
