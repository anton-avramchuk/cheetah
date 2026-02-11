using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/stack-items", ApiMethod.Create, ServiceName = "StackItems")]
public record CreateStackItemRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
