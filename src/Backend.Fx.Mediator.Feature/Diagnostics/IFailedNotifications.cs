using System.Collections;

namespace Backend.Fx.Mediator.Feature.Diagnostics;

public interface IFailedNotifications : IEnumerable<FailedNotification>
{
    internal void Add(FailedNotification failedNotification);
}

public class FailedNotifications : IFailedNotifications
{
    private readonly ThreadSafeBoundedQueue<FailedNotification> _failedNotifications = new(100);

    public IEnumerator<FailedNotification> GetEnumerator() => _failedNotifications.GetEnumerator();

    public void Add(FailedNotification failedNotification)
    {
        _failedNotifications.Enqueue(failedNotification);
    }

    IEnumerator IEnumerable.GetEnumerator() => _failedNotifications.GetEnumerator();
}