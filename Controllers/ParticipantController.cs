using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    [RoleAuthorize("Participant")]
    public class ParticipantController : AppController
    {
        private readonly IRaceDayApi _api;

        public ParticipantController(IRaceDayApi api)
        {
            _api = api;
        }

        // My enrolments and their statuses
        [HttpGet]
        public async Task<IActionResult> Enrolments()
        {
            return View(await _api.GetMyEnrolmentsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int id)
        {
            try
            {
                await _api.WithdrawEnrolmentAsync(id);
                SetSuccess("You have withdrawn from the event.");
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                SetError(ex.Message);
            }

            return RedirectToAction(nameof(Enrolments));
        }

        // Personal race history
        [HttpGet]
        public async Task<IActionResult> Results()
        {
            return View(await _api.GetMyResultsAsync());
        }
    }
}
