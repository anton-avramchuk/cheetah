using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skills", ApiMethod.GetGrid, ResponseType = typeof(SkillViewModel), ServiceName = "Skill")]
public class GetSkillsGridRequest : GridRequest;
