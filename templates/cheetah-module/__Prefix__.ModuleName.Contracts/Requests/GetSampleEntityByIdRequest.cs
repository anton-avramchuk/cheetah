using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace __Prefix__.ModuleName.Contracts.Requests;

public record GetSampleEntityByIdRequest([FromRoute] Guid Id) : ICrmRequest;
