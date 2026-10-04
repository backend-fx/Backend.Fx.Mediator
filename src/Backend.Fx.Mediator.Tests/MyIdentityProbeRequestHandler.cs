using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Util;

namespace Backend.Fx.Mediator.Tests;

public record MyIdentityProbeRequest : IRequest<IdentityProbeResponse>;

public record IdentityProbeResponse(string? IdentityName);

public class MyIdentityProbeRequestHandler : IRequestHandler<MyIdentityProbeRequest, IdentityProbeResponse>
{
    private readonly ICurrentTHolder<IIdentity> _identityHolder;

    public MyIdentityProbeRequestHandler(ICurrentTHolder<IIdentity> identityHolder)
    {
        _identityHolder = identityHolder;
    }

    public ValueTask<IdentityProbeResponse> HandleAsync(
        MyIdentityProbeRequest request,
        CancellationToken cancellation = default)
        => ValueTask.FromResult(new IdentityProbeResponse(_identityHolder.Current.Name));
}
