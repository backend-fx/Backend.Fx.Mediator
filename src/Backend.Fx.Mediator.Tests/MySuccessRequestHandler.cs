using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public record MySuccessRequest : IRequest<SuccessResponse>;

public class MySuccessRequestHandler : IRequestHandler<MySuccessRequest>
{
    public static bool WasCalled { get; private set; }

    public ValueTask<SuccessResponse> HandleAsync(
        MySuccessRequest request,
        CancellationToken cancellation = default)
    {
        WasCalled = true;
        return ValueTask.FromResult(new SuccessResponse());
    }
}
