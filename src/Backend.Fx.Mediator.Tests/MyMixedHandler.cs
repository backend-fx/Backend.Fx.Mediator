using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;

namespace Backend.Fx.Mediator.Tests;

public record MyMixedRequest : IRequest<MixedResponse>;

public record MixedResponse;

public record MyTestNotification6;

public class MyMixedHandler
    : IRequestHandler<MyMixedRequest, MixedResponse>,
        INotificationHandler<MyTestNotification6>
{
    public static IRequestHandler<MyMixedRequest, MixedResponse> RequestSpy { get; } =
        A.Fake<IRequestHandler<MyMixedRequest, MixedResponse>>();

    public ValueTask<MixedResponse> HandleAsync(
        MyMixedRequest request,
        CancellationToken cancellation = default
    )
    {
        RequestSpy.HandleAsync(request, cancellation);
        return ValueTask.FromResult(new MixedResponse());
    }

    public ValueTask HandleAsync(
        MyTestNotification6 notification,
        CancellationToken cancellation = default
    ) => ValueTask.CompletedTask;
}
