using Cheetah.Contracts.Requests;

namespace __Prefix__.ModuleName.Contracts.Requests;

public record DeleteSampleEntityRequest(Guid Id) : ICrmRequest;
