using Backend.Fx.Execution.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature.Internal;

public class FlushMediatorOutboxOperation : IOperation
{
    private readonly IOperation _operation;
    
    private readonly IMediator _mediator;

    public FlushMediatorOutboxOperation(IMediator mediator, IOperation operation)
    {
        _operation = operation;
        _mediator = mediator;
    }

    public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellation = default)
    {
        return _operation.BeginAsync(serviceScope, cancellation);
    }

    public async Task CompleteAsync(CancellationToken cancellation = default)
    {
        await _operation.CompleteAsync(cancellation);
        await ((MediatorOutbox)_mediator).FlushAsync(cancellation);
    }

    public Task CancelAsync(CancellationToken cancellation = default)
    {
        return _operation.CancelAsync(cancellation);
    }
}