using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Mediator.Tests;

public class MyRequestHandler : IRequestHandler<MyTestRequest, string>
{
    private readonly MyTestRequestSpy _spy;

    public MyRequestHandler(MyTestRequestSpy spy)
    {
        _spy = spy;
    }

    public async ValueTask<string> HandleAsync(MyTestRequest request, CancellationToken cancellation = default)
    {
        await Task.Delay(50, cancellation);
        return await _spy.RequestHandler.HandleAsync(request, cancellation);
    }
}