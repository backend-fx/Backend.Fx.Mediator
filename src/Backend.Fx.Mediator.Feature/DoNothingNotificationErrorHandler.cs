using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature;

internal class DoNothingNotificationErrorHandler : INotificationErrorHandler
{
    public void HandleError<TNotification>(
        Type handlerType,
        TNotification notification,
        IIdentity sender,
        Exception exception) where TNotification : notnull
    {
    }
}
