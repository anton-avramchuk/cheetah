using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record LocationViewModel(Guid Id, string Country, string City, string Timezone) : ICrmResponse;
