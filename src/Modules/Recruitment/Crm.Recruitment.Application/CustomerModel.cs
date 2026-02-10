namespace Crm.Recruitment.Application;

public record CustomerModel(Guid Id, string Name, string? Description, Guid? DirectionId);
