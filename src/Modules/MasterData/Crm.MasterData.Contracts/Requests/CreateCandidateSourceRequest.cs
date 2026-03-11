using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record CreateCandidateSourceRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
