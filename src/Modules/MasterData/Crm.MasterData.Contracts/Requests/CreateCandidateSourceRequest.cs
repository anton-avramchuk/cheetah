using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/candidate-sources", ApiMethod.Create, ServiceName = "CandidateSource")]
public record CreateCandidateSourceRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
