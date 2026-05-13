using System.Security.Principal;
using Backend.Fx.Execution.Pipeline;

namespace Backend.Fx.Mediator.Feature;

public class MediatorOptions
{
    /// <summary>
    /// This error handler is being executed when handling a notification fails. Default to an empty operation.
    /// </summary>
    public INotificationErrorHandler ErrorHandler { get; set; } = new DoNothingNotificationErrorHandler();

    /// <summary>
    /// The identity to assume when no specific identity is provided when calling the Invoke method of the
    /// <see cref="IBackendFxApplicationInvoker"/>. Defaults to <see cref="AnonymousIdentity"/>.
    /// </summary>
    public IIdentity DefaultRequestor { get; set; } = new AnonymousIdentity();

    /// <summary>
    /// The identity that is used to send notifications. Defaults to <see cref="SystemIdentity"/>.
    /// </summary>
    public IIdentity DefaultNotifier { get; set; } = new SystemIdentity();

    /// <summary>
    /// Use outbox for notifications that queues all notifications and eventually sends them when the operation
    /// completed successfully. Defaults to <code>true</code>.
    /// </summary>
    public bool UseOutbox { get; set; } = true;

    /// <summary>
    /// When responding to a request, should the response be automatically sent additionally as a notification to
    /// any possible subscriber? Defaults to <code>false</code>.
    /// </summary>
    public bool AutoNotifyResponses { get; set; } = false;
}