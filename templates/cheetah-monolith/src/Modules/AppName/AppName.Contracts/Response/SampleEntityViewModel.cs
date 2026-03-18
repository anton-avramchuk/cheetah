using Cheetah.Contracts.Responses;

namespace AppName.Contracts.Response;

public record SampleEntityViewModel(Guid Id, string Name, string? Description) : ICrmResponse;
