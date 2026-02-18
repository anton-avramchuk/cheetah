namespace Crm.Recruitment.Application;

public record CustomerModel(Guid Id, string Name, string? Code, string? Description, Guid? DirectionId);
