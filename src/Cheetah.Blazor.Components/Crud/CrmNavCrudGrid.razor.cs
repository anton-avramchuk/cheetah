using Cheetah.Blazor.Components.Dialogs;
using Cheetah.Contracts.Requests;
using Microsoft.AspNetCore.Components;

namespace Cheetah.Blazor.Components.Crud;

public partial class CrmNavCrudGrid<TGridViewModel>
    where TGridViewModel : IGridViewModel
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    #region Parameters

    /// <summary>
    /// Grid service for data operations (list + delete).
    /// </summary>
    [Parameter, EditorRequired]
    public IGridService<TGridViewModel> Service { get; set; } = null!;

    /// <summary>
    /// URL to navigate for creating a new item.
    /// </summary>
    [Parameter]
    public string? CreateUrl { get; set; }

    /// <summary>
    /// URL pattern for editing an item. Use {0} as placeholder for entity id.
    /// </summary>
    [Parameter]
    public string? EditUrl { get; set; }

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
    /// Enable search/filter input.
    /// </summary>
    [Parameter]
    public bool AllowSearch { get; set; } = true;

    /// <summary>
    /// Field name for search filter.
    /// </summary>
    [Parameter]
    public string SearchField { get; set; } = "Name";

    /// <summary>
    /// Delete confirmation message.
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
    /// Placeholder for search input.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Поиск...";

    /// <summary>
    /// Default page size.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Available page size options.
    /// </summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];

    /// <summary>
    /// Event callback after successful delete.
    /// </summary>
    [Parameter]
    public EventCallback<TGridViewModel> OnDeleted { get; set; }

    #endregion

    #region State

    private IReadOnlyList<TGridViewModel>? _items;
    private IReadOnlyList<GridColumnInfo> _columns = [];
    private bool _isLoading = true;
    private string? _errorMessage;

    // Pagination state
    private int _currentPage = 1;
    private int _currentPageSize;
    private int _totalItems;
    private int _totalPages => _currentPageSize > 0 ? (int)Math.Ceiling((double)_totalItems / _currentPageSize) : 1;

    // Filter state
    private string _searchText = string.Empty;
    private System.Timers.Timer? _debounceTimer;

    #endregion

    private bool HasActions => (AllowEdit && EditUrl != null) || AllowDelete;

    protected override void OnInitialized()
    {
        _columns = GridColumnInfo.FromType<TGridViewModel>();
        _currentPageSize = PageSize;
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

    private GridRequest BuildGridRequest()
    {
        var request = new GridRequest
        {
            Page = _currentPage,
            PageSize = _currentPageSize
        };

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            request.Filter = new FilterDescriptor
            {
                Field = SearchField,
                Operator = "contains",
                Value = _searchText.Trim()
            };
        }

        return request;
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            var request = BuildGridRequest();
            var result = await Service.GetAllAsync(request);
            _items = result.Data.ToList();
            _totalItems = result.Total;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Ошибка загрузки: {ex.Message}";
            _items = [];
            _totalItems = 0;
        }
        finally
        {
            _isLoading = false;
        }
    }

    #region Pagination

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > _totalPages || page == _currentPage)
            return;

        _currentPage = page;
        await LoadDataAsync();
    }

    private async Task GoToFirstPageAsync() => await GoToPageAsync(1);
    private async Task GoToPreviousPageAsync() => await GoToPageAsync(_currentPage - 1);
    private async Task GoToNextPageAsync() => await GoToPageAsync(_currentPage + 1);
    private async Task GoToLastPageAsync() => await GoToPageAsync(_totalPages);

    private async Task OnPageSizeChangedAsync(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var newSize) && newSize > 0)
        {
            _currentPageSize = newSize;
            _currentPage = 1;
            await LoadDataAsync();
        }
    }

    private IEnumerable<int> GetVisiblePageNumbers()
    {
        const int maxVisible = 5;
        var start = Math.Max(1, _currentPage - maxVisible / 2);
        var end = Math.Min(_totalPages, start + maxVisible - 1);

        if (end - start + 1 < maxVisible)
        {
            start = Math.Max(1, end - maxVisible + 1);
        }

        for (var i = start; i <= end; i++)
        {
            yield return i;
        }
    }

    #endregion

    #region Search/Filter

    private void OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;

        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        _debounceTimer = new System.Timers.Timer(300);
        _debounceTimer.Elapsed += async (_, _) =>
        {
            _debounceTimer?.Stop();
            await InvokeAsync(async () =>
            {
                _currentPage = 1;
                await LoadDataAsync();
                StateHasChanged();
            });
        };
        _debounceTimer.AutoReset = false;
        _debounceTimer.Start();
    }

    private async Task ClearSearchAsync()
    {
        _searchText = string.Empty;
        _currentPage = 1;
        await LoadDataAsync();
    }

    #endregion

    #region Navigation & Delete

    private void NavigateToCreate()
    {
        if (CreateUrl != null)
            Navigation.NavigateTo(CreateUrl);
    }

    private void NavigateToEdit(Guid id)
    {
        if (EditUrl != null)
            Navigation.NavigateTo(string.Format(EditUrl, id));
    }

    private async Task DeleteAsync(TGridViewModel item)
    {
        var result = await DialogService.ShowAsync<IComponent>(
            typeof(DeleteConfirmationDialog),
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

    #endregion

    public void Dispose()
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
    }
}
