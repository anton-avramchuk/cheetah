using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skills/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(SkillViewModel), ServiceName = "Skill")]
public record GetSkillByIdRequest([FromRoute] Guid Id) : ICrmRequest;
