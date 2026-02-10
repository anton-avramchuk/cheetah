using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetWorkFormatByIdQuery(Guid Id) : IQuery<WorkFormatModel?>;
