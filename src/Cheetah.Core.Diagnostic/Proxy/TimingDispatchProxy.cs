using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Cheetah.Core.Diagnostic.Attributes;
using Cheetah.Core.Diagnostic.Options;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Diagnostic.Proxy;

/// <summary>
/// DispatchProxy that intercepts interface method calls marked with <see cref="MeasureTimeAttribute"/>
/// and logs their execution time.
/// </summary>
internal sealed class TimingDispatchProxy : DispatchProxy
{
    private object _target = null!;
    private ILogger _logger = null!;
    private DiagnosticsOptions _options = null!;

    // Cached reflection lookup for generic log helpers
    private static readonly MethodInfo LogAfterTaskGenericMethod =
        typeof(TimingDispatchProxy).GetMethod(nameof(LogAfterTaskGeneric),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo LogAfterValueTaskGenericMethod =
        typeof(TimingDispatchProxy).GetMethod(nameof(LogAfterValueTaskGeneric),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    internal static object Create(Type serviceType, object target, ILogger logger, DiagnosticsOptions options)
    {
        var createMethod = typeof(DispatchProxy)
            .GetMethod(nameof(DispatchProxy.Create))!
            .MakeGenericMethod(serviceType, typeof(TimingDispatchProxy));

        var proxy = createMethod.Invoke(null, null)!;
        var p = (TimingDispatchProxy)proxy;
        p._target = target;
        p._logger = logger;
        p._options = options;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod is null) return null;

        if (!_options.Enabled || !HasMeasureTimeAttribute(targetMethod))
        {
            return InvokeTarget(targetMethod, args);
        }

        return MeasureInvocation(targetMethod, args);
    }

    private object? InvokeTarget(MethodInfo method, object?[]? args)
    {
        try
        {
            return method.Invoke(_target, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Throw(ex.InnerException);
            return null;
        }
    }

    private object? MeasureInvocation(MethodInfo method, object?[]? args)
    {
        var attr = GetMeasureTimeAttribute(method)!;
        var label = attr.Label is not null ? $" ({attr.Label})" : string.Empty;
        var typeName = _target.GetType().Name;
        var methodName = method.Name;

        var sw = Stopwatch.StartNew();
        object? result;

        try
        {
            result = method.Invoke(_target, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            sw.Stop();
            _logger.LogWarning(ex.InnerException,
                "[Diagnostic]{Label} {Type}.{Method} threw after {Elapsed}ms",
                label, typeName, methodName, sw.ElapsedMilliseconds);
            ExceptionDispatchInfo.Throw(ex.InnerException);
            return null;
        }

        return WrapResult(result, method.ReturnType, label, typeName, methodName, sw);
    }

    private object? WrapResult(
        object? result, Type returnType,
        string label, string typeName, string methodName,
        Stopwatch sw)
    {
        if (returnType == typeof(Task))
            return LogAfterTask((Task)result!, label, typeName, methodName, sw);

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var t = returnType.GetGenericArguments()[0];
            return LogAfterTaskGenericMethod.MakeGenericMethod(t)
                .Invoke(this, [result, label, typeName, methodName, sw])!;
        }

        if (returnType == typeof(ValueTask))
            return new ValueTask(LogAfterTask(((ValueTask)result!).AsTask(), label, typeName, methodName, sw));

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var t = returnType.GetGenericArguments()[0];
            return LogAfterValueTaskGenericMethod.MakeGenericMethod(t)
                .Invoke(this, [result, label, typeName, methodName, sw])!;
        }

        sw.Stop();
        _logger.LogInformation(
            "[Diagnostic]{Label} {Type}.{Method} completed in {Elapsed}ms",
            label, typeName, methodName, sw.ElapsedMilliseconds);
        return result;
    }

    private async Task LogAfterTask(
        Task task, string label, string typeName, string methodName, Stopwatch sw)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex,
                "[Diagnostic]{Label} {Type}.{Method} threw after {Elapsed}ms",
                label, typeName, methodName, sw.ElapsedMilliseconds);
            throw;
        }

        sw.Stop();
        _logger.LogInformation(
            "[Diagnostic]{Label} {Type}.{Method} completed in {Elapsed}ms",
            label, typeName, methodName, sw.ElapsedMilliseconds);
    }

    private async Task<T> LogAfterTaskGeneric<T>(
        Task<T> task, string label, string typeName, string methodName, Stopwatch sw)
    {
        T result;
        try
        {
            result = await task;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex,
                "[Diagnostic]{Label} {Type}.{Method} threw after {Elapsed}ms",
                label, typeName, methodName, sw.ElapsedMilliseconds);
            throw;
        }

        sw.Stop();
        _logger.LogInformation(
            "[Diagnostic]{Label} {Type}.{Method} completed in {Elapsed}ms",
            label, typeName, methodName, sw.ElapsedMilliseconds);
        return result;
    }

    private async ValueTask<T> LogAfterValueTaskGeneric<T>(
        ValueTask<T> valueTask, string label, string typeName, string methodName, Stopwatch sw)
    {
        T result;
        try
        {
            result = await valueTask;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex,
                "[Diagnostic]{Label} {Type}.{Method} threw after {Elapsed}ms",
                label, typeName, methodName, sw.ElapsedMilliseconds);
            throw;
        }

        sw.Stop();
        _logger.LogInformation(
            "[Diagnostic]{Label} {Type}.{Method} completed in {Elapsed}ms",
            label, typeName, methodName, sw.ElapsedMilliseconds);
        return result;
    }

    private bool HasMeasureTimeAttribute(MethodInfo interfaceMethod)
        => GetMeasureTimeAttribute(interfaceMethod) is not null;

    private MeasureTimeAttribute? GetMeasureTimeAttribute(MethodInfo interfaceMethod)
    {
        var attr = interfaceMethod.GetCustomAttribute<MeasureTimeAttribute>();
        if (attr is not null) return attr;

        // Fall back to the concrete implementation's method
        var implMethod = _target.GetType().GetMethod(
            interfaceMethod.Name,
            BindingFlags.Public | BindingFlags.Instance,
            interfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray());

        return implMethod?.GetCustomAttribute<MeasureTimeAttribute>();
    }
}
