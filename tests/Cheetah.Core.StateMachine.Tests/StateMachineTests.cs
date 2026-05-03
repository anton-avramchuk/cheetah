using Cheetah.Core.StateMachine;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Cheetah.Core.StateMachine.Tests;

public class StateMachineTests
{
    private enum TestState
    {
        A, B, C, D
    }

    [Fact]
    public void Builder_ShouldConfigureTransitions()
    {
        // Arrange
        var builder = new StateMachineBuilder<TestState>();

        // Act
        builder.From(TestState.A).To(TestState.B, TestState.C)
               .From(TestState.B).To(TestState.D);

        var transitions = builder.Build();

        // Assert
        transitions.ShouldContainKey(TestState.A);
        transitions[TestState.A].ShouldBe(new[] { TestState.B, TestState.C });

        transitions.ShouldContainKey(TestState.B);
        transitions[TestState.B].ShouldBe(new[] { TestState.D });

        transitions.ShouldNotContainKey(TestState.C);
    }

    [Fact]
    public void Validator_ShouldAllowValidTransitions()
    {
        // Arrange
        var options = new StateMachineOptions();
        options.For<TestState>()
               .From(TestState.A).To(TestState.B, TestState.C);

        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        var validator = new StateMachineValidator<TestState>(optionsWrapper);

        // Act & Assert
        validator.CanTransition(TestState.A, TestState.B).ShouldBeTrue();
        validator.CanTransition(TestState.A, TestState.C).ShouldBeTrue();
    }

    [Fact]
    public void Validator_ShouldDenyInvalidTransitions()
    {
        // Arrange
        var options = new StateMachineOptions();
        options.For<TestState>()
               .From(TestState.A).To(TestState.B);

        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        var validator = new StateMachineValidator<TestState>(optionsWrapper);

        // Act & Assert
        validator.CanTransition(TestState.A, TestState.C).ShouldBeFalse();
        validator.CanTransition(TestState.B, TestState.D).ShouldBeFalse();
    }

    [Fact]
    public void Validator_ValidateTransition_ShouldThrowOnInvalid()
    {
        // Arrange
        var options = new StateMachineOptions();
        options.For<TestState>()
               .From(TestState.A).To(TestState.B);

        var validator = new StateMachineValidator<TestState>(Microsoft.Extensions.Options.Options.Create(options));

        // Act
        var act = () => validator.ValidateTransition(TestState.A, TestState.C);

        // Assert
        var exception = act.ShouldThrow<InvalidStateTransitionException>();
        exception.FromState.ShouldBe(TestState.A.ToString());
        exception.ToState.ShouldBe(TestState.C.ToString());
    }

    [Fact]
    public void Validator_GetAllowedTransitions_ShouldReturnCorrectStates()
    {
        // Arrange
        var options = new StateMachineOptions();
        options.For<TestState>()
               .From(TestState.A).To(TestState.B, TestState.C);

        var validator = new StateMachineValidator<TestState>(Microsoft.Extensions.Options.Options.Create(options));

        // Act
        var allowed = validator.GetAllowedTransitions(TestState.A);

        // Assert
        allowed.ShouldBe(new[] { TestState.B, TestState.C }, ignoreOrder: true);
        validator.GetAllowedTransitions(TestState.B).ShouldBeEmpty();
    }

    [Fact]
    public void Validator_WhenNotConfigured_ShouldThrowOnCreation()
    {
        // Arrange
        var options = new StateMachineOptions();
        
        // Act
        var act = () => new StateMachineValidator<TestState>(Microsoft.Extensions.Options.Options.Create(options));

        // Assert
        act.ShouldThrow<InvalidOperationException>()
           .Message.ShouldContain("No state machine configuration found");
    }
}
