using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Calendar.Application.Reminders;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Api.Endpoints;

/// <summary>POST api/calendar-events/{eventId}/reminders — добавить напоминание (вернёт его id).</summary>
public sealed class AddReminderEndpoint : CreateCommandEndpoint<AddReminderRequest, AddReminderCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/reminders";
    public override string GetByIdRouteName => "GetEvent";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("AddReminder").WithTags("Reminders");
}

/// <summary>DELETE api/calendar-events/{eventId}/reminders/{reminderId} — удалить напоминание.</summary>
public sealed class RemoveReminderEndpoint : DeleteCommandEndpoint<RemoveReminderRequest, RemoveReminderCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/reminders/{reminderId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("RemoveReminder").WithTags("Reminders");
}
