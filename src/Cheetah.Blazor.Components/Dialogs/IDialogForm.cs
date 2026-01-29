using Microsoft.AspNetCore.Components.Forms;

namespace Cheetah.Blazor.Components.Dialogs;

/// <summary>
/// Interface for dialog form components that return a typed result.
/// </summary>
/// <typeparam name="TResult">Type of data returned when form is submitted.</typeparam>
public interface IDialogForm<TResult>
{
    /// <summary>
    /// Gets the EditContext for form validation.
    /// May be null if the dialog doesn't use form validation.
    /// </summary>
    EditContext? EditContext { get; }

    /// <summary>
    /// Submits the form and returns the result.
    /// Called when user clicks a confirm button.
    /// </summary>
    Task<DialogFormResult<TResult>> SubmitAsync();
}

/// <summary>
/// Context provided to dialog forms via CascadingParameter.
/// Allows forms to register their EditContext for validation tracking.
/// </summary>
public interface IDialogFormContext
{
    /// <summary>
    /// Registers the form's EditContext with the dialog host.
    /// Call this when EditContext is created or changed.
    /// </summary>
    void RegisterEditContext(EditContext? editContext);
}
