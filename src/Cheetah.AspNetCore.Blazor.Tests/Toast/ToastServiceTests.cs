using Cheetah.AspNetCore.Blazor.Toast;

namespace Cheetah.AspNetCore.Blazor.Tests.Toast;

public class ToastServiceTests
{
    private static (ToastService Svc, Func<IReadOnlyList<ToastMessage>?> Last) Subscribe()
    {
        var svc = new ToastService();
        IReadOnlyList<ToastMessage>? last = null;
        ((IToastHost)svc).OnToastsChanged += l => last = l;
        return (svc, () => last);
    }

    [Fact]
    public void Success_AddsToast_AndRaisesEvent()
    {
        var (svc, last) = Subscribe();

        svc.Success("saved", "Заголовок");

        var toasts = last();
        toasts.ShouldNotBeNull();
        toasts!.Count.ShouldBe(1);
        toasts[0].Type.ShouldBe(ToastType.Success);
        toasts[0].Message.ShouldBe("saved");
        toasts[0].Title.ShouldBe("Заголовок");
        toasts[0].Duration.ShouldBe(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public void Error_IsPersistent_ByDefault()
    {
        var (svc, last) = Subscribe();

        svc.Error("boom");

        last()![0].Type.ShouldBe(ToastType.Error);
        last()![0].Duration.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Info_And_Warning_HaveExpectedDefaultDurations()
    {
        var (svc, last) = Subscribe();

        svc.Info("i");
        svc.Warning("w");

        last()!.Single(t => t.Type == ToastType.Info).Duration.ShouldBe(TimeSpan.FromSeconds(4));
        last()!.Single(t => t.Type == ToastType.Warning).Duration.ShouldBe(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ExplicitDuration_OverridesDefault()
    {
        var (svc, last) = Subscribe();

        svc.Success("x", duration: TimeSpan.FromSeconds(30));

        last()![0].Duration.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Dismiss_RemovesToast_AndRaisesEvent()
    {
        var (svc, last) = Subscribe();
        var msg = new ToastMessage { Message = "x", Duration = TimeSpan.Zero };
        svc.Show(msg);
        last()!.Count.ShouldBe(1);

        svc.Dismiss(msg.Id);

        last()!.Count.ShouldBe(0);
    }

    [Fact]
    public void MultipleToasts_Accumulate_InOrder()
    {
        var (svc, last) = Subscribe();

        svc.Info("first");
        svc.Info("second");

        last()!.Count.ShouldBe(2);
        last()![0].Message.ShouldBe("first");
        last()![1].Message.ShouldBe("second");
    }
}
