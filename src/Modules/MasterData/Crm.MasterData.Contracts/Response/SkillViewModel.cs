using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record SkillViewModel(Guid Id, string Name, Guid? SkillCategoryId) : ICrmResponse;
