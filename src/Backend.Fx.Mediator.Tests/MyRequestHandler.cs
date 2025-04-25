using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyRequestHandler : IRequestHandler<MyTestRequest, string>
{
    private readonly Spy _spy;

    public MyRequestHandler(Spy spy)
    {
        _spy = spy;
    }

    public async ValueTask<string> HandleAsync(MyTestRequest request, CancellationToken cancellation = default)
    {
        await Task.Delay(50, cancellation);
        return await _spy.RequestHandler.HandleAsync(request, cancellation);
    }
}