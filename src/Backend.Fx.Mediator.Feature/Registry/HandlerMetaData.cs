using System.Reflection;
using Backend.Fx.Mediator.Api;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator.Feature.Registry;

[PublicAPI]
public record HandlerMetaData(HandlerKey Key, Type HandlerType)
{
    public ServiceDescriptor ServiceDescriptor => new(HandlerType, HandlerType, ServiceLifetime.Scoped);

    public bool IsApiGet() => HandlerType.GetCustomAttribute<ApiGetAttribute>() != null;
    
    public bool IsApiPost() => HandlerType.GetCustomAttribute<ApiPostAttribute>() != null;
    
    public bool IsApiPut() => HandlerType.GetCustomAttribute<ApiPutAttribute>() != null;
    
    public bool IsApiDelete() => HandlerType.GetCustomAttribute<ApiDeleteAttribute>() != null;
}