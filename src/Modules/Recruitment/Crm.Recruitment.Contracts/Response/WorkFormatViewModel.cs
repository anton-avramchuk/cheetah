using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record WorkFormatViewModel(Guid Id, string Name) : ICrmResponse;
