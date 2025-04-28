using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator.Feature.Internal;

[PublicAPI]
public interface INotificationErrorHandler
{
    void HandleError<TNotification>(Type handlerType, TNotification notification, IIdentity sender, Exception exception)
        where TNotification : notnull;
}

public class DoNothingNotificationErrorHandler : INotificationErrorHandler
{
    public void HandleError<TNotification>(
        Type handlerType,
        TNotification notification,
        IIdentity sender,
        Exception exception) where TNotification : notnull
    {
    }
}