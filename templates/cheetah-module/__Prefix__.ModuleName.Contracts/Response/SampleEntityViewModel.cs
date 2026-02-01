using Cheetah.Contracts.Responses;

namespace __Prefix__.ModuleName.Contracts.Response;

public record SampleEntityViewModel(Guid Id, string Name, string? Description) : ICrmResponse;
