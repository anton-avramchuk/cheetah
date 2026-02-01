using Cheetah.Backend.Endpoints.Http;
using __Prefix__.ModuleName.Application.Commands;
using __Prefix__.ModuleName.Contracts.Requests;

namespace __Prefix__.ModuleName.Api.Endpoints;

public class CreateSampleEntityEndpoint : CreateCommandEndpoint<CreateSampleEntityRequest, CreateSampleEntityCommand>
{
    public override string Route => Constants.DefaultRoute;

    public override string GetByIdRouteName => "GetSampleEntityById";
}
