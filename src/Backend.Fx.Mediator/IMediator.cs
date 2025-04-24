using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator;

[PublicAPI]
public interface IMediator : IDisposable
{
    void Notify<TNotification>(TNotification notification) where TNotification : class;
    
    ValueTask<TResponse> RequestAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>;

    ValueTask<TResponse> RequestAsync<TRequest, TResponse>(
        TRequest request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>;
}