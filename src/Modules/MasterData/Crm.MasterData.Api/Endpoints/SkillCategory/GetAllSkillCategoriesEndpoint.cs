using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.SkillCategory;

public class GetAllSkillCategoriesEndpoint : QueryGridEndpoint<GetAllSkillCategoriesRequest,
    GetAllSkillCategoriesQuery, SkillCategoryModel, SkillCategoryViewModel>
{
    public override string Route => Constants.SkillCategoriesRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SkillCategories");
    }
}
