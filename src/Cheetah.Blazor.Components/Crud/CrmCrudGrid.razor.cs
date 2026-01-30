using Cheetah.Blazor.Components.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Cheetah.Blazor.Components.Crud;

public partial class CrmCrudGrid<TGridViewModel, TCreateViewModel, TEditViewModel>
    where TGridViewModel : IGridViewModel
{
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    #region Parameters

    /// <summary>
    /// CRUD service for data operations.
    /// </summary>
    [Parameter, EditorRequired]
    public ICrudService<TGridViewModel, TCreateViewModel, TEditViewModel> Service { get; set; } = null!;

    /// <summary>
    /// Grid title.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Items";

    /// <summary>
    /// Allow creating new items.
    /// </summary>
    [Parameter]
    public bool AllowCreate { get; set; } = true;

    /// <summary>
    /// Allow editing items.
    /// </summary>
    [Parameter]
    public bool AllowEdit { get; set; } = true;

    /// <summary>
    /// Allow deleting items.
    /// </summary>
    [Parameter]
    public bool AllowDelete { get; set; } = true;

    /// <summary>
    /// Type of component for create form.
    /// Must implement IDialogForm&lt;TCreateViewModel&gt;.
    /// </summary>
    [Parameter]
    public Type? CreateFormType { get; set; }

    /// <summary>
    /// Type of component for edit form.
    /// Must implement IDialogForm&lt;TEditViewModel&gt;.
    /// </summary>
    [Parameter]
    public Type? EditFormType { get; set; }

    /// <summary>
    /// Dialog title for create.
    /// </summary>
    [Parameter]
    public string CreateDialogTitle { get; set; } = "Создание";

    /// <summary>
    /// Dialog title for edit.
    /// </summary>
    [Parameter]
    public string EditDialogTitle { get; set; } = "Редактирование";

    /// <summary>
    /// Delete confirmation message.
    /// Use {0} for item representation.
    /// </summary>
    [Parameter]
    public string DeleteConfirmMessage { get; set; } = "Вы уверены, что хотите удалить эту запись?";

    /// <summary>
    /// Text for create button.
    /// </summary>
    [Parameter]
    public string CreateButtonText { get; set; } = "Добавить";

    /// <summary>
    /// Text when no items.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "Нет данных";

    /// <summary>
    /// Text while loading.
    /// </summary>
    [Parameter]
    public string LoadingMessage { get; set; } = "Загрузка...";

    /// <summary>
    /// Header for actions column.
    /// </summary>
    [Parameter]
    public string ActionsColumnHeader { get; set; } = "Действия";

    /// <summary>
    /// Width of actions column.
    /// </summary>
    [Parameter]
    public string ActionsColumnWidth { get; set; } = "120px";

    /// <summary>
    /// Event callback after successful create.
    /// </summary>
    [Parameter]
    public EventCallback<TCreateViewModel> OnCreated { get; set; }

    /// <summary>
    /// Event callback after successful edit.
    /// </summary>
    [Parameter]
    public EventCallback<TEditViewModel> OnUpdated { get; set; }

    /// <summary>
    /// Event callback after successful delete.
    /// </summary>
    [Parameter]
    public EventCallback<TGridViewModel> OnDeleted { get; set; }

    #endregion

    private IReadOnlyList<TGridViewModel>? _items;
    private IReadOnlyList<GridColumnInfo> _columns = [];
    private bool _isLoading = true;
    private string? _errorMessage;

    private bool HasActions => (AllowEdit && EditFormType != null) || AllowDelete;

    protected override void OnInitialized()
    {
        _columns = GridColumnInfo.FromType<TGridViewModel>();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Reloads the grid data.
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadDataAsync();
        StateHasChanged();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            _items = await Service.GetAllAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OpenCreateDialogAsync()
    {
        if (CreateFormType == null)
            return;

        var result = await DialogService.ShowAsync<IComponent>(
            CreateFormType,
            parameters: null,
            options: new DialogOptions
            {
                Title = CreateDialogTitle,
                Buttons = DialogButtons.SaveCancel
            });

        if (result.Confirmed)
        {
            var model = result.GetData<TCreateViewModel>();
            if (model != null)
            {
                try
                {
                    await Service.CreateAsync(model);
                    await OnCreated.InvokeAsync(model);
                    await RefreshAsync();
                }
                catch (Exception ex)
                {
                    _errorMessage = $"Ошибка создания: {ex.Message}";
                    StateHasChanged();
                }
            }
        }
    }

    private async Task OpenEditDialogAsync(Guid id)
    {
        if (EditFormType == null)
            return;

        var parameters = new Dictionary<string, object?>
        {
            ["Id"] = id
        };

        var result = await DialogService.ShowAsync<IComponent>(
            EditFormType,
            parameters: parameters,
            options: new DialogOptions
            {
                Title = EditDialogTitle,
                Buttons = DialogButtons.SaveCancel
            });

        if (result.Confirmed)
        {
            var model = result.GetData<TEditViewModel>();
            if (model != null)
            {
                try
                {
                    await Service.UpdateAsync(id, model);
                    await OnUpdated.InvokeAsync(model);
                    await RefreshAsync();
                }
                catch (Exception ex)
                {
                    _errorMessage = $"Ошибка сохранения: {ex.Message}";
                    StateHasChanged();
                }
            }
        }
    }

    private async Task DeleteAsync(TGridViewModel item)
    {
        var result = await DialogService.ShowAsync<DeleteConfirmationDialog>(
            parameters: new Dictionary<string, object?>
            {
                ["Message"] = DeleteConfirmMessage
            },
            options: new DialogOptions
            {
                Title = "Подтверждение удаления",
                Buttons = DialogButtons.DeleteCancel
            });

        if (result.Confirmed)
        {
            try
            {
                await Service.DeleteAsync(item.Id);
                await OnDeleted.InvokeAsync(item);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                _errorMessage = $"Ошибка удаления: {ex.Message}";
                StateHasChanged();
            }
        }
    }
}
