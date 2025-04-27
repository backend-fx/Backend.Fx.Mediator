using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator;

[PublicAPI]
public interface IMediator : IAsyncDisposable, IDisposable
{
    ValueTask Notify<TNotification>(TNotification notification, CancellationToken cancellation = default) 
        where TNotification : class;
    
    ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class;

    ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class;
}