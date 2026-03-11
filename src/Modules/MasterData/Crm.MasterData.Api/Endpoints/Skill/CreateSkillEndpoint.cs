using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Skill;

public class CreateSkillEndpoint : CreateCommandEndpoint<CreateSkillRequest, CreateSkillCommand>
{
    public override string Route => Constants.SkillsRoute;
    public override string GetByIdRouteName => "GetSkillById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Skills");
    }
}
