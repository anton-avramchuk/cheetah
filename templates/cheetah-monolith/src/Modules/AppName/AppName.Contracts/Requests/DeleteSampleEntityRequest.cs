using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace AppName.Contracts.Requests;

public record DeleteSampleEntityRequest([FromRoute] Guid Id) : ICrmRequest;
