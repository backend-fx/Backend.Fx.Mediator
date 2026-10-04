using System;
using Backend.Fx.Mediator.Feature.Registry;
using Shouldly;
using Xunit;

namespace Backend.Fx.Mediator.Tests;

public class TheHandlerRegistry
{
    private readonly HandlerRegistry _sut;

    public TheHandlerRegistry()
    {
        _sut = new HandlerRegistry([GetType().Assembly]);
    }

    [Fact]
    public void ComplainsAboutMultipleHandlersForTheSameRequest()
    {
        var exception = Should.Throw<InvalidOperationException>(
            () => _sut.GetRequestHandlerType<AmbiguousResponse>(typeof(MyAmbiguousRequest)));

        exception.Message.ShouldContain(nameof(MyAmbiguousRequestHandler1));
        exception.Message.ShouldContain(nameof(MyAmbiguousRequestHandler2));
    }

    [Fact]
    public void ComplainsAboutMissingHandlerForARequest()
    {
        var exception = Should.Throw<InvalidOperationException>(
            () => _sut.GetRequestHandlerType<AmbiguousResponse>(typeof(MyTestRequest)));

        exception.Message.ShouldContain("No handler found");
    }
}
