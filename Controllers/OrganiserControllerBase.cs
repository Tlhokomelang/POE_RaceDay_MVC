using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    // Shared plumbing for the organiser screens.
    public abstract class OrganiserControllerBase : AppController
    {
        protected readonly IRaceDayApi Api;
        protected readonly ISessionUser CurrentUser;

        protected OrganiserControllerBase(IRaceDayApi api, ISessionUser user)
        {
            Api = api;
            CurrentUser = user;
        }

        // Loads an event and confirms it belongs to the signed-in organiser.
        // Returns null when it belongs to someone else.
        protected async Task<EventDetail?> LoadOwnedEventAsync(int eventId)
        {
            EventDetail ev = await Api.GetEventAsync(eventId);

            return ev.OrganiserID == CurrentUser.UserId ? ev : null;
        }

        protected async Task<EventPickerViewModel> BuildPickerAsync(
            string title, string subtitle, string controller, string buttonText)
        {
            var events = await Api.GetMyEventsAsync();

            return new EventPickerViewModel
            {
                Title = title,
                Subtitle = subtitle,
                Controller = controller,
                ButtonText = buttonText,
                Events = events
            };
        }
    }
}
