using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.SkillCategory;

public class CreateSkillCategoryEndpoint : CreateCommandEndpoint<CreateSkillCategoryRequest, CreateSkillCategoryCommand>
{
    public override string Route => Constants.SkillCategoriesRoute;
    public override string GetByIdRouteName => "GetSkillCategoryById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SkillCategories");
    }
}
