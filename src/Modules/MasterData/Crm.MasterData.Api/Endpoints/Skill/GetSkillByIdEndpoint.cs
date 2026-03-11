using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.Skill;

public class GetSkillByIdEndpoint : QueryOrNotFoundEndpoint<GetSkillByIdRequest, GetSkillByIdQuery,
    SkillModel, SkillViewModel>
{
    public override string Route => $"{Constants.SkillsRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetSkillById");
        config.WithTags("Skills");
    }
}
