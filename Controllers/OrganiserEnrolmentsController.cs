using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    [RoleAuthorize("Organiser")]
    public class OrganiserEnrolmentsController : OrganiserControllerBase
    {
        public OrganiserEnrolmentsController(IRaceDayApi api, ISessionUser user)
            : base(api, user)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? eventId)
        {
            if (eventId == null)
            {
                return View("Picker", await BuildPickerAsync(
                    "Enrolments",
                    "Choose an event to see who has enrolled.",
                    "OrganiserEnrolments",
                    "View enrolments"));
            }

            EventDetail? ev = await LoadOwnedEventAsync(eventId.Value);

            if (ev == null) return RedirectToAction("AccessDenied", "Account");

            return View(new EnrolmentsPageViewModel
            {
                Event = ev,
                Enrolments = await Api.GetEventEnrolmentsAsync(eventId.Value)
            });
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(
            int enrolmentId, int eventId, string status)
        {
            try
            {
                await Api.SetEnrolmentStatusAsync(enrolmentId, status);
                SetSuccess($"Enrolment marked as {status}.");
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                SetError(ex.Message);
            }

            return RedirectToAction(nameof(Index), new { eventId });
        }
    }
}
