using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace AppName.Contracts.Requests;

public record GetSampleEntityByIdRequest([FromRoute] Guid Id) : ICrmRequest;
