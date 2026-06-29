using Bunit;
using Cheetah.Contracts.Requests;
using Cheetah.AspNetCore.Blazor.Dialogs;
using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Tests.Grid;

public class CrmGridTests : BunitContext
{
    private readonly FakeCrudService _service = new();
    private readonly StubDialogService _dialog = new();

    public CrmGridTests()
    {
        Services.AddSingleton<IDialogService>(_dialog);
        Services.AddSingleton<IToastService>(new NullToastService());
    }

    private IRenderedComponent<CrmGrid<FakeGridViewModel, FakeDetailsViewModel, FakeCreateViewModel>> RenderGrid(
        Action<ComponentParameterCollectionBuilder<CrmGrid<FakeGridViewModel, FakeDetailsViewModel, FakeCreateViewModel>>> extra)
        => Render<CrmGrid<FakeGridViewModel, FakeDetailsViewModel, FakeCreateViewModel>>(p =>
        {
            p.Add(g => g.Service, _service);
            extra(p);
        });

    private NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    // ---- Создание ----------------------------------------------------------

    [Fact]
    public void Create_ViaDialog_EmitsOnCreated_WithNewId()
    {
        var newId = Guid.NewGuid();
        _service.CreateReturns = newId;
        Guid? created = null;

        var cut = RenderGrid(p => p
            .Add(g => g.CreateFormType, typeof(FakeCreateViewModel))
            .Add(g => g.OnCreated, (Guid id) => created = id));

        cut.Find(".crm-btn-primary").Click();

        _service.CreateCalls.ShouldBe(1);
        created.ShouldBe(newId);
    }

    [Fact]
    public void CreateUrl_NavigatesToPage_AndSkipsDialog()
    {
        var cut = RenderGrid(p => p.Add(g => g.CreateUrl, "/things/new"));

        cut.Find(".crm-btn-primary").Click();

        Nav.Uri.ShouldEndWith("/things/new");
        _dialog.ShowCalls.ShouldBe(0);
        _service.CreateCalls.ShouldBe(0);
    }

    [Fact]
    public void ShowCreate_False_HidesCreateButton()
    {
        var cut = RenderGrid(p => p.Add(g => g.ShowCreate, false));

        cut.FindAll(".crm-btn-primary").Count.ShouldBe(0);
    }

    // ---- Редактирование ----------------------------------------------------

    [Fact]
    public void EditUrl_NavigatesToPage_AndSkipsDialog()
    {
        var id = Guid.NewGuid();
        _service.Items.Add(new FakeGridViewModel { Id = id, Name = "row" });

        var cut = RenderGrid(p => p.Add(g => g.EditUrl, rid => $"/things/{rid}"));

        cut.Find(".crm-btn-edit").Click();

        Nav.Uri.ShouldEndWith($"/things/{id}");
        _dialog.ShowCalls.ShouldBe(0);
    }

    [Fact]
    public void Edit_ViaDialog_EmitsOnUpdated_WithRowId()
    {
        var id = Guid.NewGuid();
        _service.Items.Add(new FakeGridViewModel { Id = id, Name = "row" });
        Guid? updated = null;

        var cut = RenderGrid(p => p
            .Add(g => g.EditFormType, typeof(FakeDetailsViewModel))
            .Add(g => g.OnUpdated, (Guid x) => updated = x));

        cut.Find(".crm-btn-edit").Click();

        _service.LastUpdatedId.ShouldBe(id);
        updated.ShouldBe(id);
    }

    // ---- Удаление ----------------------------------------------------------

    [Fact]
    public void Delete_Confirmed_EmitsOnDeleted_WithRowId()
    {
        var id = Guid.NewGuid();
        _service.Items.Add(new FakeGridViewModel { Id = id, Name = "row" });
        _dialog.ConfirmResult = true;
        Guid? deleted = null;

        var cut = RenderGrid(p => p.Add(g => g.OnDeleted, (Guid x) => deleted = x));

        cut.Find(".crm-btn-danger").Click();

        _service.DeleteCalls.ShouldBe(1);
        deleted.ShouldBe(id);
    }

    [Fact]
    public void Delete_Cancelled_DoesNotDelete()
    {
        _service.Items.Add(new FakeGridViewModel { Id = Guid.NewGuid(), Name = "row" });
        _dialog.ConfirmResult = false;

        var cut = RenderGrid(p => { });

        cut.Find(".crm-btn-danger").Click();

        _service.DeleteCalls.ShouldBe(0);
    }

    // ---- Колонка действий --------------------------------------------------

    [Fact]
    public void ActionsColumn_Hidden_WhenNoEditDeleteOrRowActions()
    {
        _service.Items.Add(new FakeGridViewModel { Id = Guid.NewGuid(), Name = "row" });

        var cut = RenderGrid(p => p
            .Add(g => g.ShowEdit, false)
            .Add(g => g.ShowDelete, false));

        cut.FindAll(".crm-grid-actions-header").Count.ShouldBe(0);
        cut.FindAll(".crm-grid-actions").Count.ShouldBe(0);
        cut.FindAll(".crm-btn-edit").Count.ShouldBe(0);
        cut.FindAll(".crm-btn-danger").Count.ShouldBe(0);
    }

    // ---- Сортировка по заголовкам -----------------------------------------

    [Fact]
    public void ClickHeader_CyclesSort_AscDescOff()
    {
        _service.Items.Add(new FakeGridViewModel { Id = Guid.NewGuid(), Name = "a" });
        var cut = RenderGrid(p => { });

        cut.Find("th.crm-grid-sortable").Click();
        _service.LastRequest!.Sort.Single().Field.ShouldBe("Name");
        _service.LastRequest.Sort.Single().Dir.ShouldBe("asc");

        cut.Find("th.crm-grid-sortable").Click();
        _service.LastRequest!.Sort.Single().Dir.ShouldBe("desc");

        cut.Find("th.crm-grid-sortable").Click();
        _service.LastRequest!.Sort.ShouldBeEmpty();
    }

    [Fact]
    public void Sortable_False_DisablesHeaderSorting()
    {
        _service.Items.Add(new FakeGridViewModel { Id = Guid.NewGuid(), Name = "a" });

        var cut = RenderGrid(p => p.Add(g => g.Sortable, false));

        cut.FindAll("th.crm-grid-sortable").Count.ShouldBe(0);
    }

    // ---- Дефолтный фильтр --------------------------------------------------

    [Fact]
    public void DefaultFilter_IsSentWithEveryRequest()
    {
        var filter = new FilterDescriptor { Field = "Name", Operator = "eq", Value = "a" };

        RenderGrid(p => p.Add(g => g.DefaultFilter, filter));

        _service.LastRequest!.Filter.ShouldBe(filter);
    }

    [Fact]
    public void ChangingDefaultFilter_ReloadsFromFirstPage()
    {
        var first = new FilterDescriptor { Field = "Name", Operator = "eq", Value = "a" };
        var cut = RenderGrid(p => p.Add(g => g.DefaultFilter, first));
        _service.LastRequest!.Filter.ShouldBe(first);

        var second = new FilterDescriptor { Field = "Name", Operator = "eq", Value = "b" };
        cut.Render(p => p.Add(g => g.DefaultFilter, second));

        _service.LastRequest!.Filter.ShouldBe(second);
        _service.LastRequest.Page.ShouldBe(1);
    }

    // ---- Фейки -------------------------------------------------------------

    private sealed class FakeCrudService : ICrudService<FakeGridViewModel, FakeDetailsViewModel, FakeCreateViewModel>
    {
        public readonly List<FakeGridViewModel> Items = [];
        public Guid CreateReturns = Guid.NewGuid();
        public int CreateCalls, DeleteCalls;
        public Guid? LastUpdatedId, LastDeletedId;
        public CrmPageRequest? LastRequest;

        public Task<CrmGridResult<FakeGridViewModel>> GetGridAsync(CrmPageRequest request, CancellationToken ct = default)
        {
            LastRequest = request;
            return Task.FromResult(new CrmGridResult<FakeGridViewModel> { Data = Items, Total = Items.Count });
        }

        public Task<FakeDetailsViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult<FakeDetailsViewModel?>(new FakeDetailsViewModel());

        public Task<Guid> CreateAsync(FakeCreateViewModel model, CancellationToken ct = default)
        {
            CreateCalls++;
            return Task.FromResult(CreateReturns);
        }

        public Task UpdateAsync(Guid id, FakeDetailsViewModel model, CancellationToken ct = default)
        {
            LastUpdatedId = id;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            DeleteCalls++;
            LastDeletedId = id;
            return Task.CompletedTask;
        }
    }

    // Исполняет primary-команду (как реальный диалог) и закрывается её commandId.
    private sealed class StubDialogService : IDialogService
    {
        public bool ConfirmResult = true;
        public int ShowCalls;

        public Task AlertAsync(string message, string title = "") => Task.CompletedTask;

        public Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
            => Task.FromResult(ConfirmResult);

        public async Task<DialogResult> ShowAsync(DialogOptions options)
        {
            ShowCalls++;
            var ctx = new FakeContext();
            var primary = options.Commands.FirstOrDefault(c => c.IsPrimary);
            if (primary is null)
                return new DialogResult("cancel");

            await primary.ExecuteAsync(ctx);
            return new DialogResult(ctx.ClosedWith ?? primary.Id);
        }

        private sealed class FakeContext : IDialogContext
        {
            public string? ClosedWith;
            public Task CloseAsync(string commandId) { ClosedWith = commandId; return Task.CompletedTask; }
            public void ShowError(string? message) { }
        }
    }

    private sealed class NullToastService : IToastService
    {
        public void Show(ToastMessage message) { }
        public void Success(string message, string? title = null, TimeSpan? duration = null) { }
        public void Info(string message, string? title = null, TimeSpan? duration = null) { }
        public void Warning(string message, string? title = null, TimeSpan? duration = null) { }
        public void Error(string message, string? title = null, TimeSpan? duration = null) { }
        public void Dismiss(Guid id) { }
    }
}
