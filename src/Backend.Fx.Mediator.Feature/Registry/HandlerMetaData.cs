using System.Reflection;
using Backend.Fx.Mediator.Api;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator.Feature.Registry;

[PublicAPI]
public record HandlerMetaData(Type HandlerType, Type RequestType, Type ResponseType)
{
    public bool IsApiGet() => HandlerType.GetCustomAttribute<ApiGetAttribute>() != null;
    
    public bool IsApiPost() => HandlerType.GetCustomAttribute<ApiPostAttribute>() != null;
    
    public bool IsApiPut() => HandlerType.GetCustomAttribute<ApiPutAttribute>() != null;
    
    public bool IsApiDelete() => HandlerType.GetCustomAttribute<ApiDeleteAttribute>() != null;
}