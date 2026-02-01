using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace __Prefix__.ModuleName.Contracts.Requests;

public record DeleteSampleEntityRequest([FromRoute] Guid Id) : ICrmRequest;
