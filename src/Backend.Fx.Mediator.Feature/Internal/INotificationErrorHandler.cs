using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator.Feature.Internal;

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
