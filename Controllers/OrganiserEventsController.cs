using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Helpers;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    [RoleAuthorize("Organiser")]
    public class OrganiserEventsController : OrganiserControllerBase
    {
        public OrganiserEventsController(IRaceDayApi api, ISessionUser user)
            : base(api, user)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await Api.GetMyEventsAsync());
        }

        // ---------- Create ----------

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View("Form", new EventFormViewModel
            {
                EventTypes = await Api.GetEventTypesAsync()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventFormViewModel model)
        {
            ValidateBanner(model);

            if (!ModelState.IsValid) return await ReturnFormAsync(model);

            try
            {
                int eventId = await Api.CreateEventAsync(ToRequest(model));

                string? bannerProblem = await TryUploadBannerAsync(eventId, model);

                if (bannerProblem == null)
                    SetSuccess("Event created. Now add the categories people can enrol in.");
                else
                    SetWarning($"Event created, but the banner could not be uploaded: {bannerProblem}");

                return RedirectToAction(
                    "Index", "OrganiserCategories", new { eventId });
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return await ReturnFormAsync(model);
            }
        }

        // ---------- Edit ----------

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            EventDetail? ev = await LoadOwnedEventAsync(id);

            if (ev == null) return RedirectToAction("AccessDenied", "Account");

            return View("Form", new EventFormViewModel
            {
                EventID = ev.EventID,
                EventName = ev.EventName,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                Distance = ev.Distance,
                EventType = ev.EventType,
                CurrentBanner = ev.BannerImage,
                EventTypes = await Api.GetEventTypesAsync()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EventFormViewModel model)
        {
            model.EventID = id;

            ValidateBanner(model);

            if (!ModelState.IsValid) return await ReturnFormAsync(model);

            try
            {
                await Api.UpdateEventAsync(id, ToRequest(model));

                string? bannerProblem = await TryUploadBannerAsync(id, model);

                if (bannerProblem == null)
                    SetSuccess("Event updated.");
                else
                    SetWarning($"Event updated, but the banner could not be uploaded: {bannerProblem}");

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return await ReturnFormAsync(model);
            }
        }

        // ---------- Delete ----------

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await Api.DeleteEventAsync(id);
                SetSuccess("Event deleted.");
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                SetError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------- helpers ----------

        private void ValidateBanner(EventFormViewModel model)
        {
            string? problem = UploadValidator.Validate(model.Banner);

            if (problem != null)
                ModelState.AddModelError(nameof(model.Banner), problem);
        }

        // Reloads the lists and the current banner, which are not posted back.
        private async Task<IActionResult> ReturnFormAsync(EventFormViewModel model)
        {
            model.EventTypes = await Api.GetEventTypesAsync();

            if (model.EventID.HasValue)
            {
                try
                {
                    EventDetail current = await Api.GetEventAsync(model.EventID.Value);
                    model.CurrentBanner = current.BannerImage;
                }
                catch (ApiException)
                {
                }
            }

            return View("Form", model);
        }

        private static EventApiRequest ToRequest(EventFormViewModel model) => new()
        {
            EventName = model.EventName.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim(),
            EventDate = model.EventDate!.Value,
            Location = model.Location.Trim(),
            Distance = model.Distance!.Value,
            EventType = model.EventType
        };

        // The event is already saved by the time we get here, so a banner
        // failure is reported as a warning instead of failing the request.
        private async Task<string?> TryUploadBannerAsync(
            int eventId, EventFormViewModel model)
        {
            if (model.Banner == null || model.Banner.Length == 0) return null;

            try
            {
                await Api.UploadBannerAsync(eventId, model.Banner);

                return null;
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                return ex.Message;
            }
        }
    }
}
