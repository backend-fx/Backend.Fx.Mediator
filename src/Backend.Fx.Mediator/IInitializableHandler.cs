namespace Backend.Fx.Mediator;

public interface IInitializableHandler
{
    Task InitializeAsync(CancellationToken cancellation);
}