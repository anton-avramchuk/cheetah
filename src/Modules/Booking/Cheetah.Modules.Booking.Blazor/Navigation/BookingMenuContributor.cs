using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Booking.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля Booking: секция «Запись».</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class BookingMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("booking", "Запись", order: 45);

        section.AddItem("booking-list", "Бронирования")
            .WithIcon("bi bi-calendar-check-fill")
            .WithUrl("bookings")
            .WithOrder(0);

        return Task.CompletedTask;
    }
}
