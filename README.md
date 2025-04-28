# Backend.Fx.Mediator

A simple mediator pattern implementation to be used with Backend.Fx.Execution.

Some features:

- notification outbox: handlers are called when the operation was completed
- optionally convert all responses into additional notifications
- optional identity propagation

### Usage

Add a reference to `Backend.Fx.Mediator.Feature` in your startup project (or whereever your `IBackendFxApplication` type lives).

Add a reference to `Backend.Fx.Mediator` in all projects that need to use the `IMediator`.

Enable the feature by adding an instance of `MediatorFeature` to your `IBackendFxApplication`. Use the delegate to configure the feature.

```csharp
_application.EnableFeature(new MediatorFeature(opt =>
    {
        // This error handler is being executed when handling a notification fails
        // Note that any failure of a _request_ handler is propagated!
        opt.ErrorHandler = new BackOffAndRetry();

        // The identity to assume when no specific identity is provided when calling 
        // the Invoke method of the application
        opt.DefaultRequestor = new GenericIdentity("Someone");

        // The identity that is used to send notifications
        opt.DefaultNotifier = new SystemIdentity();

        // When responding to a request, should the response be automatically sent 
        // additionally as notification to any possible subscriber?
        opt.AutoNotifyResponses = true;
    }));
```
