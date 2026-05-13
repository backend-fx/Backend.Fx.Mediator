namespace Backend.Fx.Mediator.Feature.Outbox;

internal interface IMediatorOutbox : IDisposable, IAsyncDisposable
{
    void Enqueue(Func<CancellationToken, Task> notification);
    
    ValueTask FlushAsync(CancellationToken cancellation);
}