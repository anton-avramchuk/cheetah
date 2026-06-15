using Cheetah.Modules.Activities.Application.Abstractions;
using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Application.Tests;

/// <summary>Конкретная активность наследника с доп. полем — для проверки generic-хендлеров.</summary>
public sealed class TestActivity : ActivityBase
{
    public string? CallOutcome { get; private set; }

    private TestActivity() { }

    public static TestActivity Create(TestCreateRequest r)
    {
        var a = new TestActivity();
        a.InitializeCore(Guid.NewGuid(), r.Type, r.Title, r.AssigneeId, r.OwnerId,
            r.EntityType, r.EntityId, r.DueAt, r.Priority, r.Description);
        a.CallOutcome = r.CallOutcome;
        return a;
    }
}

public sealed record TestCreateRequest : CreateActivityRequestBase
{
    public string? CallOutcome { get; init; }
}

public sealed record TestUpdateRequest : UpdateActivityRequestBase;

public sealed record TestActivityDto : ActivityDtoBase
{
    public string? CallOutcome { get; init; }
}

public sealed class TestActivityFactory : IActivityFactory<TestActivity, TestCreateRequest>
{
    public TestActivity Create(TestCreateRequest request) => TestActivity.Create(request);
}

public sealed class TestActivityProjector : IActivityProjector<TestActivity, TestActivityDto>
{
    public TestActivityDto ToDto(TestActivity a) => new()
    {
        Id = a.Id,
        Type = a.Type,
        Title = a.Title,
        Description = a.Description,
        Status = a.Status,
        Priority = a.Priority,
        AssigneeId = a.AssigneeId,
        OwnerId = a.OwnerId,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        DueAt = a.DueAt,
        CompletedAt = a.CompletedAt,
        Result = a.Result,
        CalendarEventId = a.CalendarEventId,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        CallOutcome = a.CallOutcome
    };
}

internal static class TestData
{
    public static TestCreateRequest CreateRequest(string title = "Call client") => new()
    {
        Type = ActivityType.Call,
        Title = title,
        AssigneeId = Guid.NewGuid(),
        OwnerId = Guid.NewGuid(),
        EntityType = EntityRefKeys.Deal,
        EntityId = Guid.NewGuid()
    };

    public static TestActivity NewActivity() => TestActivity.Create(CreateRequest());
}
