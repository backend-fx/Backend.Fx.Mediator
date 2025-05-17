using Backend.Fx.Execution.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature.Outbox;

internal class FlushMediatorOutboxOperation : IOperation
{
    private readonly IOperation _operation;
    
    private readonly IMediatorOutbox _outbox;

    public FlushMediatorOutboxOperation(IMediatorOutbox outbox, IOperation operation)
    {
        _operation = operation;
        _outbox = outbox;
    }

    public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellation = default)
    {
        return _operation.BeginAsync(serviceScope, cancellation);
    }

    public async Task CompleteAsync(CancellationToken cancellation = default)
    {
        await _operation.CompleteAsync(cancellation);
        await _outbox.FlushAsync(cancellation);
    }

    public Task CancelAsync(CancellationToken cancellation = default)
    {
        return _operation.CancelAsync(cancellation);
    }
}