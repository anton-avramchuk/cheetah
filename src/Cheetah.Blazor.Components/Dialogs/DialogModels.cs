using Cheetah.Blazor.Components.Modals;

namespace Cheetah.Blazor.Components.Dialogs;

/// <summary>
/// Result from a dialog form's SubmitAsync method.
/// </summary>
/// <typeparam name="TResult">Type of data returned on success.</typeparam>
public readonly struct DialogFormResult<TResult>
{
    public bool Success { get; }
    public TResult? Data { get; }
    public string? ErrorMessage { get; }

    private DialogFormResult(bool success, TResult? data, string? errorMessage)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static DialogFormResult<TResult> Ok(TResult data) => new(true, data, null);
    public static DialogFormResult<TResult> Fail(string error) => new(false, default, error);
}

/// <summary>
/// Result returned when a dialog is closed.
/// </summary>
public sealed class DialogResult
{
    public bool Confirmed { get; }
    public object? Data { get; }

    private DialogResult(bool confirmed, object? data)
    {
        Confirmed = confirmed;
        Data = data;
    }

    public static DialogResult Ok(object? data = null) => new(true, data);
    public static DialogResult Cancel() => new(false, null);

    public TResult? GetData<TResult>() => Data is TResult result ? result : default;
}

/// <summary>
/// Defines a button to display in dialog footer.
/// </summary>
public sealed class DialogButton
{
    public string Text { get; set; } = "";
    public string? Icon { get; set; }
    public ColorVariant Variant { get; set; } = ColorVariant.Secondary;
    public bool IsConfirm { get; set; }
    public bool IsCancel { get; set; }
    public bool DisableWhenInvalid { get; set; } = true;
}

/// <summary>
/// Preset button configurations for common dialog scenarios.
/// </summary>
public static class DialogButtons
{
    public static IReadOnlyList<DialogButton> SaveCancel =>
    [
        new DialogButton
        {
            Text = "Отмена",
            Variant = ColorVariant.Secondary,
            IsCancel = true,
            DisableWhenInvalid = false
        },
        new DialogButton
        {
            Text = "Сохранить",
            Icon = "check",
            Variant = ColorVariant.Primary,
            IsConfirm = true,
            DisableWhenInvalid = true
        }
    ];

    public static IReadOnlyList<DialogButton> OkCancel =>
    [
        new DialogButton
        {
            Text = "Отмена",
            Variant = ColorVariant.Secondary,
            IsCancel = true,
            DisableWhenInvalid = false
        },
        new DialogButton
        {
            Text = "OK",
            Variant = ColorVariant.Primary,
            IsConfirm = true,
            DisableWhenInvalid = true
        }
    ];

    public static IReadOnlyList<DialogButton> YesNo =>
    [
        new DialogButton
        {
            Text = "Нет",
            Variant = ColorVariant.Secondary,
            IsCancel = true,
            DisableWhenInvalid = false
        },
        new DialogButton
        {
            Text = "Да",
            Variant = ColorVariant.Primary,
            IsConfirm = true,
            DisableWhenInvalid = false
        }
    ];

    public static IReadOnlyList<DialogButton> DeleteCancel =>
    [
        new DialogButton
        {
            Text = "Отмена",
            Variant = ColorVariant.Secondary,
            IsCancel = true,
            DisableWhenInvalid = false
        },
        new DialogButton
        {
            Text = "Удалить",
            Icon = "trash",
            Variant = ColorVariant.Danger,
            IsConfirm = true,
            DisableWhenInvalid = false
        }
    ];
}

/// <summary>
/// Options for configuring dialog appearance and behavior.
/// </summary>
public sealed class DialogOptions
{
    public string? Title { get; set; }
    public CrmModal.ModalSize Size { get; set; } = CrmModal.ModalSize.Default;
    public bool Centered { get; set; }
    public bool Scrollable { get; set; }
    public bool ShowCloseButton { get; set; } = true;
    public bool CloseOnBackdropClick { get; set; } = false;
    public IReadOnlyList<DialogButton>? Buttons { get; set; }
}
