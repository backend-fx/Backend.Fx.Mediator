namespace Backend.Fx.Mediator;

public interface IInitializableHandler
{
    ValueTask InitializeAsync(CancellationToken cancellation = default);
}

public interface IInitializableHandler<in TRequest>
    where TRequest : IRequest
{
    ValueTask InitializeAsync(TRequest request, CancellationToken cancellation = default);
}
