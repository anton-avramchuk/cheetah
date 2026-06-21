using Cheetah.AspNetCore.Blazor.Dialogs;
using Moq;

namespace Cheetah.AspNetCore.Blazor.Tests.Dialogs;

public class DialogServiceTests
{
    private static (DialogService Svc, Func<IReadOnlyList<ActiveDialog>?> Last) Subscribe()
    {
        var svc = new DialogService();
        IReadOnlyList<ActiveDialog>? last = null;
        ((IDialogHost)svc).OnDialogChanged += l => last = l;
        return (svc, () => last);
    }

    [Fact]
    public void ShowAsync_PushesDialog_AndLeavesTaskPending()
    {
        var (svc, last) = Subscribe();

        var task = svc.ShowAsync(new DialogOptions { Title = "t" });

        task.IsCompleted.ShouldBeFalse();
        last()!.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Close_CompletesTask_AndRemovesFromStack()
    {
        var (svc, last) = Subscribe();
        var task = svc.ShowAsync(new DialogOptions { Title = "t" });
        var dialog = last()![0];

        ((IDialogHost)svc).Close(dialog, "ok");

        var result = await task;
        result.IsOk.ShouldBeTrue();
        last()!.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AlertAsync_UsesOkOnlyCommands()
    {
        var (svc, last) = Subscribe();

        var task = svc.AlertAsync("сообщение", "Заголовок");
        var dialog = last()![0];

        dialog.Options.Commands.ShouldBe(DialogCommands.OkOnly);

        ((IDialogHost)svc).Close(dialog, "ok");
        await task;
    }

    [Fact]
    public async Task ConfirmAsync_ReturnsTrue_OnOk()
    {
        var (svc, last) = Subscribe();
        var task = svc.ConfirmAsync("точно?");
        var dialog = last()![0];

        ((IDialogHost)svc).Close(dialog, "ok");

        (await task).ShouldBeTrue();
    }

    [Fact]
    public async Task ConfirmAsync_ReturnsFalse_OnCancel()
    {
        var (svc, last) = Subscribe();
        var task = svc.ConfirmAsync("точно?");
        var dialog = last()![0];

        ((IDialogHost)svc).Close(dialog, "cancel");

        (await task).ShouldBeFalse();
    }

    [Fact]
    public void Stack_ClosingInner_KeepsOuterOpenAndPending()
    {
        var (svc, last) = Subscribe();
        var outer = svc.ShowAsync(new DialogOptions { Title = "outer" });
        var inner = svc.ShowAsync(new DialogOptions { Title = "inner" });
        last()!.Count.ShouldBe(2);

        var innerDialog = last()![1];
        ((IDialogHost)svc).Close(innerDialog, "ok");

        last()!.Count.ShouldBe(1);
        outer.IsCompleted.ShouldBeFalse();
        inner.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public void DialogResult_MapsCommandIdToFlags()
    {
        new DialogResult("ok").IsOk.ShouldBeTrue();
        new DialogResult("ok").IsCancelled.ShouldBeFalse();
        new DialogResult("cancel").IsCancelled.ShouldBeTrue();
        new DialogResult("custom").IsOk.ShouldBeFalse();
    }

    [Fact]
    public async Task DialogCommand_DefaultExecute_ClosesWithItsId()
    {
        var ctx = new Mock<IDialogContext>();

        await DialogCommands.Ok.ExecuteAsync(ctx.Object);

        ctx.Verify(c => c.CloseAsync("ok"), Times.Once);
    }

    [Fact]
    public async Task DialogCommand_CustomOnExecute_IsInvoked_InsteadOfDefaultClose()
    {
        var ctx = new Mock<IDialogContext>();
        var invoked = false;
        var cmd = new DialogCommand("save", "Сохранить", OnExecute: _ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await cmd.ExecuteAsync(ctx.Object);

        invoked.ShouldBeTrue();
        ctx.Verify(c => c.CloseAsync(It.IsAny<string>()), Times.Never);
    }
}
