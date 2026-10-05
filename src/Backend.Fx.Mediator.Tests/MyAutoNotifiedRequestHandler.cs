using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public record MyAutoNotifiedRequest : IRequest<AutoNotifiedResponse>;

public record AutoNotifiedResponse;

public class MyAutoNotifiedRequestHandler
    : IRequestHandler<MyAutoNotifiedRequest, AutoNotifiedResponse>
{
    public ValueTask<AutoNotifiedResponse> HandleAsync(
        MyAutoNotifiedRequest request,
        CancellationToken cancellation = default
    ) => ValueTask.FromResult(new AutoNotifiedResponse());
}

public class MyAutoNotifiedResponseHandler : INotificationHandler<AutoNotifiedResponse>
{
    public static bool HasCompleted { get; private set; }

    public async ValueTask HandleAsync(
        AutoNotifiedResponse notification,
        CancellationToken cancellation = default
    )
    {
        await Task.Delay(50, cancellation);
        HasCompleted = true;
    }
}
