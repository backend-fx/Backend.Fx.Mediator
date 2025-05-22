using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Mediator.Api;

namespace Backend.Fx.Mediator.Tests;

[ApiGet]
public class MyAuthorizedRequestHandler1 : IRequestHandler<MyAuthorizedRequest1, string>, IAuthorizedHandler
{
    private readonly MyAuthorizedRequestSpy _spy;

    public MyAuthorizedRequestHandler1(MyAuthorizedRequestSpy spy)
    {
        _spy = spy;
    }

    public async ValueTask<string> HandleAsync(MyAuthorizedRequest1 request, CancellationToken cancellation = default)
    {
        await Task.Delay(50, cancellation);
        return "hello";
    }

    public ValueTask<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation = default)
    {
        return _spy.IsAuthorizedAsync(identity, cancellation);
    }
}