using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class FailingNotificationHandler : INotificationHandler<FailingNotification>
{
    public ValueTask HandleAsync(FailingNotification notification, CancellationToken cancellation = default)
    {
        throw new DivideByZeroException();
    }
}