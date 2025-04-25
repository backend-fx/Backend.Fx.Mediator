using System.Security.Principal;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Mediator.Feature.Internal;

namespace Backend.Fx.Mediator.Feature;

public class MediatorOptions
{
    public INotificationErrorHandler ErrorHandler { get; set; } = new DoNothingNotificationErrorHandler();

    public IIdentity DefaultRequestor { get; set; } = new AnonymousIdentity();
}