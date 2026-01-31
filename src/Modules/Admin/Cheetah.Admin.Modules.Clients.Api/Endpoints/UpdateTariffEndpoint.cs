using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Backend.Endpoints.Http;

namespace Cheetah.Admin.Modules.Clients.Api.Endpoints;

public class UpdateTariffEndpoint : UpdateCommandEndpoint<UpdateTariffRequest, UpdateTariffCommand>
{
    public override string Route => $"{Constants.TariffsRoute}/{{id:guid}}";
}
