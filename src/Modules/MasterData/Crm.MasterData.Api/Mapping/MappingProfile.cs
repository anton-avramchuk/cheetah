using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;
using Mapster;

namespace Crm.MasterData.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // StackItem
        config.NewConfig<StackItemModel, StackItemViewModel>();
        config.NewConfig<GetSampleEntitiesGridRequest, GetSampleEntitiesGridQuery>();
        config.NewConfig<GetStackItemByIdRequest, GetStackItemByIdQuery>();
        config.NewConfig<CreateStackItemRequest, CreateStackItemCommand>();
        config.NewConfig<UpdateStackItemRequest, UpdateStackItemCommand>();
        config.NewConfig<DeleteStackItemRequest, DeleteStackItemCommand>();

        // Industry
        config.NewConfig<IndustryModel, IndustryViewModel>();
        config.NewConfig<GetIndustriesGridRequest, GetIndustriesGridQuery>();
        config.NewConfig<GetIndustryByIdRequest, GetIndustryByIdQuery>();
        config.NewConfig<CreateIndustryRequest, CreateIndustryCommand>();
        config.NewConfig<UpdateIndustryRequest, UpdateIndustryCommand>();
        config.NewConfig<DeleteIndustryRequest, DeleteIndustryCommand>();

        // Position
        config.NewConfig<PositionModel, PositionViewModel>();
        config.NewConfig<GetPositionsGridRequest, GetPositionsGridQuery>();
        config.NewConfig<GetPositionByIdRequest, GetPositionByIdQuery>();
        config.NewConfig<CreatePositionRequest, CreatePositionCommand>();
        config.NewConfig<UpdatePositionRequest, UpdatePositionCommand>();
        config.NewConfig<DeletePositionRequest, DeletePositionCommand>();

        // WorkFormat
        config.NewConfig<WorkFormatModel, WorkFormatViewModel>();
        config.NewConfig<GetWorkFormatsGridRequest, GetWorkFormatsGridQuery>();
        config.NewConfig<GetWorkFormatByIdRequest, GetWorkFormatByIdQuery>();
        config.NewConfig<CreateWorkFormatRequest, CreateWorkFormatCommand>();
        config.NewConfig<UpdateWorkFormatRequest, UpdateWorkFormatCommand>();
        config.NewConfig<DeleteWorkFormatRequest, DeleteWorkFormatCommand>();

        // CandidateSource
        config.NewConfig<CandidateSourceModel, CandidateSourceViewModel>();
        config.NewConfig<GetCandidateSourcesGridRequest, GetCandidateSourcesGridQuery>();
        config.NewConfig<GetCandidateSourceByIdRequest, GetCandidateSourceByIdQuery>();
        config.NewConfig<CreateCandidateSourceRequest, CreateCandidateSourceCommand>();
        config.NewConfig<UpdateCandidateSourceRequest, UpdateCandidateSourceCommand>();
        config.NewConfig<DeleteCandidateSourceRequest, DeleteCandidateSourceCommand>();

        // SkillCategory
        config.NewConfig<SkillCategoryModel, SkillCategoryViewModel>();
        config.NewConfig<GetSkillCategoriesGridRequest, GetSkillCategoriesGridQuery>();
        config.NewConfig<GetSkillCategoryByIdRequest, GetSkillCategoryByIdQuery>();
        config.NewConfig<CreateSkillCategoryRequest, CreateSkillCategoryCommand>();
        config.NewConfig<UpdateSkillCategoryRequest, UpdateSkillCategoryCommand>();
        config.NewConfig<DeleteSkillCategoryRequest, DeleteSkillCategoryCommand>();

        // Skill
        config.NewConfig<SkillModel, SkillViewModel>();
        config.NewConfig<GetSkillsGridRequest, GetSkillsGridQuery>();
        config.NewConfig<GetSkillByIdRequest, GetSkillByIdQuery>();
        config.NewConfig<CreateSkillRequest, CreateSkillCommand>();
        config.NewConfig<UpdateSkillRequest, UpdateSkillCommand>();
        config.NewConfig<DeleteSkillRequest, DeleteSkillCommand>();

        // Location
        config.NewConfig<LocationModel, LocationViewModel>();
        config.NewConfig<GetLocationsGridRequest, GetLocationsGridQuery>();
        config.NewConfig<GetLocationByIdRequest, GetLocationByIdQuery>();
        config.NewConfig<CreateLocationRequest, CreateLocationCommand>();
        config.NewConfig<UpdateLocationRequest, UpdateLocationCommand>();
        config.NewConfig<DeleteLocationRequest, DeleteLocationCommand>();
    }
}
