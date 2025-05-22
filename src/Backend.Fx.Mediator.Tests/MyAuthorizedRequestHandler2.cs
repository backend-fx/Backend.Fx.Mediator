using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Mediator.Api;

namespace Backend.Fx.Mediator.Tests;

[ApiGet]
public class MyAuthorizedRequestHandler2 : IRequestHandler<MyAuthorizedRequest2, string>, IAuthorizedHandler<MyAuthorizedRequest2>
{
    private readonly MyAuthorizedRequestSpy _spy;

    public MyAuthorizedRequestHandler2(MyAuthorizedRequestSpy spy)
    {
        _spy = spy;
    }

    public async ValueTask<string> HandleAsync(MyAuthorizedRequest2 request, CancellationToken cancellation = default)
    {
        await Task.Delay(50, cancellation);
        return "hello";
    }

    public ValueTask<bool> IsAuthorizedAsync(IIdentity identity, MyAuthorizedRequest2 request, CancellationToken cancellation = default)
    {
        return _spy.IsAuthorizedAsync(identity, request, cancellation);
    }
}