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
        config.NewConfig<GetAllSampleEntitiesRequest, GetAllSampleEntitiesQuery>();
        config.NewConfig<GetStackItemByIdRequest, GetStackItemByIdQuery>();
        config.NewConfig<CreateStackItemRequest, CreateStackItemCommand>();
        config.NewConfig<UpdateStackItemRequest, UpdateStackItemCommand>();
        config.NewConfig<DeleteStackItemRequest, DeleteStackItemCommand>();

        // Industry
        config.NewConfig<IndustryModel, IndustryViewModel>();
        config.NewConfig<GetAllIndustriesRequest, GetAllIndustriesQuery>();
        config.NewConfig<GetIndustryByIdRequest, GetIndustryByIdQuery>();
        config.NewConfig<CreateIndustryRequest, CreateIndustryCommand>();
        config.NewConfig<UpdateIndustryRequest, UpdateIndustryCommand>();
        config.NewConfig<DeleteIndustryRequest, DeleteIndustryCommand>();

        // Position
        config.NewConfig<PositionModel, PositionViewModel>();
        config.NewConfig<GetAllPositionsRequest, GetAllPositionsQuery>();
        config.NewConfig<GetPositionByIdRequest, GetPositionByIdQuery>();
        config.NewConfig<CreatePositionRequest, CreatePositionCommand>();
        config.NewConfig<UpdatePositionRequest, UpdatePositionCommand>();
        config.NewConfig<DeletePositionRequest, DeletePositionCommand>();

        // WorkFormat
        config.NewConfig<WorkFormatModel, WorkFormatViewModel>();
        config.NewConfig<GetAllWorkFormatsRequest, GetAllWorkFormatsQuery>();
        config.NewConfig<GetWorkFormatByIdRequest, GetWorkFormatByIdQuery>();
        config.NewConfig<CreateWorkFormatRequest, CreateWorkFormatCommand>();
        config.NewConfig<UpdateWorkFormatRequest, UpdateWorkFormatCommand>();
        config.NewConfig<DeleteWorkFormatRequest, DeleteWorkFormatCommand>();

        // CandidateSource
        config.NewConfig<CandidateSourceModel, CandidateSourceViewModel>();
        config.NewConfig<GetAllCandidateSourcesRequest, GetAllCandidateSourcesQuery>();
        config.NewConfig<GetCandidateSourceByIdRequest, GetCandidateSourceByIdQuery>();
        config.NewConfig<CreateCandidateSourceRequest, CreateCandidateSourceCommand>();
        config.NewConfig<UpdateCandidateSourceRequest, UpdateCandidateSourceCommand>();
        config.NewConfig<DeleteCandidateSourceRequest, DeleteCandidateSourceCommand>();

        // SkillCategory
        config.NewConfig<SkillCategoryModel, SkillCategoryViewModel>();
        config.NewConfig<GetAllSkillCategoriesRequest, GetAllSkillCategoriesQuery>();
        config.NewConfig<GetSkillCategoryByIdRequest, GetSkillCategoryByIdQuery>();
        config.NewConfig<CreateSkillCategoryRequest, CreateSkillCategoryCommand>();
        config.NewConfig<UpdateSkillCategoryRequest, UpdateSkillCategoryCommand>();
        config.NewConfig<DeleteSkillCategoryRequest, DeleteSkillCategoryCommand>();

        // Skill
        config.NewConfig<SkillModel, SkillViewModel>();
        config.NewConfig<GetAllSkillsRequest, GetAllSkillsQuery>();
        config.NewConfig<GetSkillByIdRequest, GetSkillByIdQuery>();
        config.NewConfig<CreateSkillRequest, CreateSkillCommand>();
        config.NewConfig<UpdateSkillRequest, UpdateSkillCommand>();
        config.NewConfig<DeleteSkillRequest, DeleteSkillCommand>();

        // Location
        config.NewConfig<LocationModel, LocationViewModel>();
        config.NewConfig<GetAllLocationsRequest, GetAllLocationsQuery>();
        config.NewConfig<GetLocationByIdRequest, GetLocationByIdQuery>();
        config.NewConfig<CreateLocationRequest, CreateLocationCommand>();
        config.NewConfig<UpdateLocationRequest, UpdateLocationCommand>();
        config.NewConfig<DeleteLocationRequest, DeleteLocationCommand>();
    }
}
