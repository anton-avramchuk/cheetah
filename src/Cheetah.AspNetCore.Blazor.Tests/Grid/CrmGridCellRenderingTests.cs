using Bunit;
using Cheetah.Contracts.Requests;
using Cheetah.AspNetCore.Blazor.Dialogs;
using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Toast;
using Cheetah.Core.Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Tests.Grid;

public class CrmGridCellRenderingTests : BunitContext
{
    private readonly RichCrudService _service = new();

    public CrmGridCellRenderingTests()
    {
        Services.AddSingleton<IDialogService>(new NoopDialogService());
        Services.AddSingleton<IToastService>(new NoopToastService());
        _service.Items.Add(new RichVm
        {
            Id = Guid.NewGuid(),
            Name = "Anna Smirnova",
            IsActive = true,
            CreatedAt = new DateTime(2026, 7, 7, 14, 8, 0),
            Note = null,
        });
    }

    private IRenderedComponent<CrmGrid<RichVm, RichVm, RichVm>> RenderGrid(
        Action<ComponentParameterCollectionBuilder<CrmGrid<RichVm, RichVm, RichVm>>>? extra = null)
        => Render<CrmGrid<RichVm, RichVm, RichVm>>(p =>
        {
            p.Add(g => g.Service, _service);
            p.Add(g => g.ShowEdit, false);
            p.Add(g => g.ShowDelete, false);
            extra?.Invoke(p);
        });

    [Fact]
    public void BoolColumn_RendersStatusPill_InsteadOfTrueFalse()
    {
        var cut = RenderGrid();

        var pill = cut.Find("td span.crm-status");
        pill.ClassList.ShouldContain("crm-status-success");
        pill.TextContent.ShouldContain("Да");
        cut.Markup.ShouldNotContain(">True<");
    }

    [Fact]
    public void NullValue_RendersMutedDash()
    {
        var cut = RenderGrid();
        cut.FindAll("td span.crm-cell-muted").ShouldNotBeEmpty();
        cut.Find("td span.crm-cell-muted").TextContent.ShouldBe("—");
    }

    [Fact]
    public void DateColumn_IsFormatted()
    {
        var cut = RenderGrid();
        cut.Markup.ShouldContain("07.07.2026 14:08");
    }

    [Fact]
    public void ColumnTemplate_OverridesDefaultRendering()
    {
        var templates = new Dictionary<string, RenderFragment<RichVm>>
        {
            ["Name"] = row => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-name");
                builder.AddContent(2, $"★ {row.Name}");
                builder.CloseElement();
            },
        };

        var cut = RenderGrid(p => p.Add(g => g.ColumnTemplates, templates));

        var custom = cut.Find("td span.custom-name");
        custom.TextContent.ShouldBe("★ Anna Smirnova");
    }

    // ---- Fakes --------------------------------------------------------------

    public sealed class RichVm : IHasId
    {
        public Guid Id { get; set; }
        [GridColumn("Имя", order: 0)] public string Name { get; set; } = string.Empty;
        [GridColumn("Активна", order: 1)] public bool IsActive { get; set; }
        [GridColumn("Создан", order: 2)] public DateTime CreatedAt { get; set; }
        [GridColumn("Заметка", order: 3)] public string? Note { get; set; }
    }

    private sealed class RichCrudService : ICrudService<RichVm, RichVm, RichVm>
    {
        public readonly List<RichVm> Items = [];

        public Task<CrmGridResult<RichVm>> GetGridAsync(CrmPageRequest request, CancellationToken ct = default)
            => Task.FromResult(new CrmGridResult<RichVm> { Data = Items, Total = Items.Count });

        public Task<RichVm?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult<RichVm?>(Items.FirstOrDefault(x => x.Id == id));

        public Task<Guid> CreateAsync(RichVm model, CancellationToken ct = default) => Task.FromResult(Guid.NewGuid());
        public Task UpdateAsync(Guid id, RichVm model, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class NoopDialogService : IDialogService
    {
        public Task AlertAsync(string message, string title = "") => Task.CompletedTask;
        public Task<bool> ConfirmAsync(string message, string title = "Подтверждение") => Task.FromResult(true);
        public Task<DialogResult> ShowAsync(DialogOptions options) => Task.FromResult(new DialogResult("cancel"));
    }

    private sealed class NoopToastService : IToastService
    {
        public void Show(ToastMessage message) { }
        public void Success(string message, string? title = null, TimeSpan? duration = null) { }
        public void Info(string message, string? title = null, TimeSpan? duration = null) { }
        public void Warning(string message, string? title = null, TimeSpan? duration = null) { }
        public void Error(string message, string? title = null, TimeSpan? duration = null) { }
        public void Dismiss(Guid id) { }
    }
}
