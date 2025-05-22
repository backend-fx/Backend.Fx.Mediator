using System.Security.Principal;

namespace Backend.Fx.Mediator;

public interface IAuthorizedHandler
{
   ValueTask<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation = default);
}

public interface IAuthorizedHandler<in TRequest> where TRequest : IRequest
{
   ValueTask<bool> IsAuthorizedAsync(IIdentity identity, TRequest request, CancellationToken cancellation = default);
}