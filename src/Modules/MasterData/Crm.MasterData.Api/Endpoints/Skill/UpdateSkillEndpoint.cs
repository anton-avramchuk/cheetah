using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Skill;

public class UpdateSkillEndpoint : UpdateCommandEndpoint<UpdateSkillRequest, UpdateSkillCommand>
{
    public override string Route => $"{Constants.SkillsRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Skills");
    }
}
