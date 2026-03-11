using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record SkillCategoryViewModel(Guid Id, string Name) : ICrmResponse;
