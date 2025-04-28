using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyAuthorizedRequestHandler : IRequestHandler<MyAuthorizedRequest, string>, IAuthorizedHandler
{
    private readonly MyAuthorizedRequestSpy _spy;

    public MyAuthorizedRequestHandler(MyAuthorizedRequestSpy spy)
    {
        _spy = spy;
    }

    public async ValueTask<string> HandleAsync(MyAuthorizedRequest request, CancellationToken cancellation = default)
    {
        await Task.Delay(50, cancellation);
        return "hello";
    }

    public Task<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation = default)
    {
        return _spy.IsAuthorizedAsync(identity, cancellation);
    }
}