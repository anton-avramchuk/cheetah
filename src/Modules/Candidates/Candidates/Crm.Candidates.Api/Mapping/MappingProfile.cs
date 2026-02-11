using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Candidates.Application;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Application.Queries;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;
using Mapster;

namespace Crm.Candidates.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Candidate
        config.NewConfig<CandidateModel, CandidateViewModel>();
        config.NewConfig<GetAllCandidatesRequest, GetAllCandidatesQuery>();
        config.NewConfig<GetCandidateByIdRequest, GetCandidateByIdQuery>();
        config.NewConfig<CreateCandidateRequest, CreateCandidateCommand>();
        config.NewConfig<UpdateCandidateRequest, UpdateCandidateCommand>();
        config.NewConfig<DeleteCandidateRequest, DeleteCandidateCommand>();

        // CandidateStage
        config.NewConfig<CandidateStageModel, CandidateStageViewModel>();
        config.NewConfig<GetCandidateStageByIdRequest, GetCandidateStageByIdQuery>();
        config.NewConfig<GetAllCandidateStagesRequest, GetAllCandidateStagesQuery>();
        config.NewConfig<CreateCandidateStageRequest, CreateCandidateStageCommand>();
        config.NewConfig<UpdateCandidateStageRequest, UpdateCandidateStageCommand>();
        config.NewConfig<DeleteCandidateStageRequest, DeleteCandidateStageCommand>();

        // CandidateSource
        config.NewConfig<CandidateSourceModel, CandidateSourceViewModel>();
        config.NewConfig<GetCandidateSourceByIdRequest, GetCandidateSourceByIdQuery>();
        config.NewConfig<GetAllCandidateSourcesRequest, GetAllCandidateSourcesQuery>();
        config.NewConfig<CreateCandidateSourceRequest, CreateCandidateSourceCommand>();
        config.NewConfig<UpdateCandidateSourceRequest, UpdateCandidateSourceCommand>();
        config.NewConfig<DeleteCandidateSourceRequest, DeleteCandidateSourceCommand>();

        // CandidateApplication
        config.NewConfig<CandidateApplicationModel, CandidateApplicationViewModel>();
        config.NewConfig<GetCandidateApplicationByIdRequest, GetCandidateApplicationByIdQuery>();
        config.NewConfig<GetAllCandidateApplicationsRequest, GetAllCandidateApplicationsQuery>();
        config.NewConfig<CreateCandidateApplicationRequest, CreateCandidateApplicationCommand>();
        config.NewConfig<DeleteCandidateApplicationRequest, DeleteCandidateApplicationCommand>();
        config.NewConfig<MoveCandidateApplicationRequest, MoveCandidateApplicationCommand>();
    }
}
