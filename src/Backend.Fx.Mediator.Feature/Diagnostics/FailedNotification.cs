using System.Security.Principal;
using NodaTime;

namespace Backend.Fx.Mediator.Feature.Diagnostics;

public record FailedNotification(Instant Timestamp, object Notification, IIdentity Notifier, Exception Exception);