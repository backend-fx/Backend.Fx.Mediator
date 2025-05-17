namespace Backend.Fx.Mediator.Feature.Outbox;

internal interface IMediatorOutbox : IDisposable, IAsyncDisposable
{
    void Enqueue(Func<CancellationToken, ValueTask> notification);
    
    ValueTask FlushAsync(CancellationToken cancellation);
}