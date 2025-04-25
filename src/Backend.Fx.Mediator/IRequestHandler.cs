using JetBrains.Annotations;

namespace Backend.Fx.Mediator;

[PublicAPI]
public interface IRequestHandler<in TRequest> where TRequest : IRequest<SuccessResponse>
{
    ValueTask<SuccessResponse> HandleAsync(TRequest request, CancellationToken cancellation = default);
}


[PublicAPI]
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellation = default);
}