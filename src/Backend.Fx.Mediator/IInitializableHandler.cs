namespace Backend.Fx.Mediator;

public interface IInitializableHandler
{
    ValueTask InitializeAsync(CancellationToken cancellation);
}