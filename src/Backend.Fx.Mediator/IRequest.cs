using JetBrains.Annotations;

namespace Backend.Fx.Mediator;

[PublicAPI]
public interface IRequest;

[PublicAPI]
public interface IRequest<out TResponse> : IRequest;