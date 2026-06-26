using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.SalesDocuments.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля SalesDocuments: секция «Документы».</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class SalesDocumentsMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("sales-documents", "Документы", order: 50);

        section.AddItem("sales-documents-list", "Документы продаж")
            .WithIcon("bi bi-file-earmark-text-fill")
            .WithUrl("sales-documents")
            .WithOrder(0);

        return Task.CompletedTask;
    }
}
