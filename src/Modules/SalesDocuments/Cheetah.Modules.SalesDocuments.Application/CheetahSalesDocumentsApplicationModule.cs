using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain;
using Cheetah.Modules.SalesDocuments.DomainEvents;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Application;

/// <summary>
/// Прикладной слой шаблонного модуля SalesDocuments: generic CQRS документов + конечный автомат
/// статуса (<see cref="DocumentStatus"/>). Закрытые generic-handler'ы регистрирует наследник через
/// <c>AddSalesDocumentsApplication&lt;…&gt;()</c>. Переходы статуса валидируются <c>IStateMachineValidator</c>;
/// допустимость зависит ещё и от <c>DocType</c> (доменные guard'ы).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmGridModule),
    typeof(CrmStateMachineModule),
    typeof(CheetahSalesDocumentsDomainModule),
    typeof(CheetahSalesDocumentsContractsModule),
    typeof(CheetahSalesDocumentsDomainEventsModule))]
public partial class CheetahSalesDocumentsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // Конечный автомат статуса документа (единый по всем DocType; допустимость уточняют guard'ы домена).
        services.AddStateMachine<DocumentStatus>(sm => sm
            .From(DocumentStatus.Draft).To(DocumentStatus.Sent, DocumentStatus.Confirmed,
                DocumentStatus.Issued, DocumentStatus.Cancelled)
            .From(DocumentStatus.Sent).To(DocumentStatus.Accepted, DocumentStatus.Rejected,
                DocumentStatus.Expired, DocumentStatus.Cancelled)
            .From(DocumentStatus.Issued).To(DocumentStatus.Paid, DocumentStatus.Overdue, DocumentStatus.Cancelled)
            .From(DocumentStatus.Overdue).To(DocumentStatus.Paid, DocumentStatus.Cancelled)
            .From(DocumentStatus.Confirmed).To(DocumentStatus.Fulfilled, DocumentStatus.Cancelled));

        RegisterServices(services);
    }
}
