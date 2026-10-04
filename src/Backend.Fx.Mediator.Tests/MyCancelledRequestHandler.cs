using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public record MyCancelledRequest : IRequest<SuccessResponse>;

public record MyCancelledNotification;

public class MyCancelledNotificationHandler : INotificationHandler<MyCancelledNotification>
{
    public static bool WasCalled { get; private set; }

    public ValueTask HandleAsync(MyCancelledNotification notification, CancellationToken cancellation = default)
    {
        WasCalled = true;
        return ValueTask.CompletedTask;
    }
}

public class MyCancelledRequestHandler : IRequestHandler<MyCancelledRequest>
{
    private readonly IMediator _mediator;

    public MyCancelledRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask<SuccessResponse> HandleAsync(
        MyCancelledRequest request,
        CancellationToken cancellation = default)
    {
        await _mediator.NotifyAsync(new MyCancelledNotification(), cancellation);
        throw new DivideByZeroException();
    }
}
