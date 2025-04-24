using JetBrains.Annotations;

namespace Backend.Fx.Mediator;

[PublicAPI]
public interface INotificationHandler<in TNotification>
{
    ValueTask HandleAsync(TNotification notification, CancellationToken cancellation = default);
}