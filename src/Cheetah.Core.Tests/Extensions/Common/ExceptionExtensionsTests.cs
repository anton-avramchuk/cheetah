using Cheetah.Core.Extensions.Common;
using Cheetah.Core.Logging;
using Shouldly;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Tests.Extensions.Common;

public class ExceptionExtensionsTests
{
    [Fact]
    public void ReThrow_ShouldPreserveStackTrace()
    {
        // Arrange
        Exception? capturedException = null;

        try
        {
            ThrowException();
        }
        catch (Exception ex)
        {
            capturedException = ex;
        }

        // Act & Assert
        capturedException.ShouldNotBeNull();
        var act = () => capturedException!.ReThrow();

        Should.Throw<InvalidOperationException>(act).StackTrace.ShouldContain(nameof(ThrowException));
    }

    [Fact]
    public void GetLogLevel_WhenExceptionImplementsIHasLogLevel_ShouldReturnCustomLevel()
    {
        // Arrange
        var exception = new CustomLogLevelException(LogLevel.Warning);

        // Act
        var logLevel = exception.GetLogLevel();

        // Assert
        logLevel.ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public void GetLogLevel_WhenExceptionDoesNotImplementIHasLogLevel_ShouldReturnDefaultLevel()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var logLevel = exception.GetLogLevel();

        // Assert
        logLevel.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void GetLogLevel_WithCustomDefaultLevel_ShouldReturnCustomDefault()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var logLevel = exception.GetLogLevel(LogLevel.Critical);

        // Assert
        logLevel.ShouldBe(LogLevel.Critical);
    }

    [Fact]
    public void GetLogLevel_WithIHasLogLevelAndCustomDefault_ShouldReturnIHasLogLevelValue()
    {
        // Arrange
        var exception = new CustomLogLevelException(LogLevel.Information);

        // Act
        var logLevel = exception.GetLogLevel(LogLevel.Critical);

        // Assert
        logLevel.ShouldBe(LogLevel.Information);
    }

    // Helper methods and classes
    private static void ThrowException()
    {
        throw new InvalidOperationException("Test exception");
    }

    private class CustomLogLevelException : Exception, IHasLogLevel
    {
        public CustomLogLevelException(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        public LogLevel LogLevel { get; set; }
    }
}
