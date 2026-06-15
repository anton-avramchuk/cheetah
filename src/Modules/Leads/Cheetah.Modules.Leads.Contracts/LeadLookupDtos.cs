using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Leads.Contracts;

/// <summary>Элемент справочника статусов лида.</summary>
public sealed record LeadStatusDto(
    Guid Id, string Code, string Name, int Order, bool IsTerminal, bool IsActive) : ICrmResponse;

/// <summary>Элемент справочника источников лида.</summary>
public sealed record LeadSourceDto(
    Guid Id, string Code, string Name, int Order, bool IsActive) : ICrmResponse;
