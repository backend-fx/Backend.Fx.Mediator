using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyInitializedRequestHandler : IRequestHandler<MyInitializedRequest, string>, IInitializableHandler
{
    private readonly MyInitializedRequestSpy _spy;

    public MyInitializedRequestHandler(MyInitializedRequestSpy spy)
    {
        _spy = spy;
    }

    public async ValueTask<string> HandleAsync(MyInitializedRequest request, CancellationToken cancellation = default)
    {
        await Task.Delay(50, cancellation);
        return "hello";
    }

    public ValueTask InitializeAsync(CancellationToken cancellation)
    {
        return _spy.InitializeAsync(cancellation);
    }
}