using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public record MyAmbiguousRequest : IRequest<AmbiguousResponse>;

public record AmbiguousResponse;

public class MyAmbiguousRequestHandler1 : IRequestHandler<MyAmbiguousRequest, AmbiguousResponse>
{
    public ValueTask<AmbiguousResponse> HandleAsync(
        MyAmbiguousRequest request,
        CancellationToken cancellation = default)
        => ValueTask.FromResult(new AmbiguousResponse());
}

public class MyAmbiguousRequestHandler2 : IRequestHandler<MyAmbiguousRequest, AmbiguousResponse>
{
    public ValueTask<AmbiguousResponse> HandleAsync(
        MyAmbiguousRequest request,
        CancellationToken cancellation = default)
        => ValueTask.FromResult(new AmbiguousResponse());
}
