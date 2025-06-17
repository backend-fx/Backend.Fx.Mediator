using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;

namespace Backend.Fx.Mediator.Tests;

public class MyTestResponseHandler : INotificationHandler<TestResponse>
{
    public static readonly INotificationHandler<TestResponse> Spy = A.Fake<INotificationHandler<TestResponse>>();
    
    public ValueTask HandleAsync(TestResponse notification, CancellationToken cancellation = default)
    {
        return Spy.HandleAsync(notification, cancellation);    
    }
}