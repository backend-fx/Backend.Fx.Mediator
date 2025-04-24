using System.Security.Principal;

namespace Backend.Fx.Mediator;

public interface IAuthorizedHandler
{
    Task<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation = default);
}