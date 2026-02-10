using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

public record CreateWorkFormatRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
