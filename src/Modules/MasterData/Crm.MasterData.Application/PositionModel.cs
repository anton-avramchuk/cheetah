using Crm.MasterData.Domain;

namespace Crm.MasterData.Application;

public record PositionModel(Guid Id, string Name, Grade Grade);
