using Cheetah.Backend.Endpoints.Http;
using __Prefix__.ModuleName.Application.Commands;
using __Prefix__.ModuleName.Contracts.Requests;

namespace __Prefix__.ModuleName.Api.Endpoints;

public class UpdateSampleEntityEndpoint : UpdateCommandEndpoint<UpdateSampleEntityRequest, UpdateSampleEntityCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";
}
