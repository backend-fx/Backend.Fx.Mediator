using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator;

[PublicAPI]
public interface INotificationErrorHandler
{
    void HandleError<TNotification>(Type handlerType, TNotification notification, IIdentity sender, Exception exception)
        where TNotification : notnull;
}