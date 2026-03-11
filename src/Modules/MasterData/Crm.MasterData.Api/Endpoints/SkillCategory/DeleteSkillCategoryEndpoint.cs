using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.SkillCategory;

public class DeleteSkillCategoryEndpoint : DeleteCommandEndpoint<DeleteSkillCategoryRequest, DeleteSkillCategoryCommand>
{
    public override string Route => $"{Constants.SkillCategoriesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SkillCategories");
    }
}
