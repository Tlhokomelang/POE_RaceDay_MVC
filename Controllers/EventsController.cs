using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    // Participant view of a single event, and the enrol action.
    [RoleAuthorize("Participant")]
    public class EventsController : AppController
    {
        private readonly IRaceDayApi _api;

        public EventsController(IRaceDayApi api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            EventDetail ev = await _api.GetEventAsync(id);
            var mine = await _api.GetMyEnrolmentsAsync();

            return View(new EventDetailsViewModel
            {
                Event = ev,
                ExistingEnrolment = mine.FirstOrDefault(e =>
                    e.EventID == id && e.Status != "Cancelled")
            });
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (categoryId <= 0)
            {
                SetError("Please choose a category before enrolling.");
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            try
            {
                await _api.CreateEnrolmentAsync(categoryId);

                SetSuccess("You're in! Your enrolment has been recorded and is awaiting confirmation by the organiser.");

                return RedirectToAction("Enrolments", "Participant");
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                SetError(ex.Message);

                return RedirectToAction(nameof(Details), new { id = eventId });
            }
        }
    }
}
