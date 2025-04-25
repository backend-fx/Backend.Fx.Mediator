using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public record FailingRequest : IRequest<SuccessResponse>;

public class FailingRequestHandler : IRequestHandler<FailingRequest, SuccessResponse>
{
    public ValueTask<SuccessResponse> HandleAsync(FailingRequest request, CancellationToken cancellation = default)
    {
        throw new DivideByZeroException();
    }
}