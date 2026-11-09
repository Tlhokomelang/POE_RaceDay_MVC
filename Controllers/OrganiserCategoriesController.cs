using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    [RoleAuthorize("Organiser")]
    public class OrganiserCategoriesController : OrganiserControllerBase
    {
        public OrganiserCategoriesController(IRaceDayApi api, ISessionUser user)
            : base(api, user)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? eventId)
        {
            if (eventId == null)
            {
                return View("Picker", await BuildPickerAsync(
                    "Categories",
                    "Choose an event to manage its age and distance categories.",
                    "OrganiserCategories",
                    "Manage categories"));
            }

            EventDetail? ev = await LoadOwnedEventAsync(eventId.Value);

            if (ev == null) return RedirectToAction("AccessDenied", "Account");

            return View(new CategoriesPageViewModel
            {
                Event = ev,
                Form = new CategoryFormViewModel
                {
                    EventID = ev.EventID,
                    Distance = ev.Distance,
                    EntryFee = 0
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryFormViewModel form)
        {
            EventDetail? ev = await LoadOwnedEventAsync(form.EventID);

            if (ev == null) return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                try
                {
                    await Api.CreateCategoryAsync(new CategoryApiRequest
                    {
                        EventID = form.EventID,
                        CategoryName = form.CategoryName.Trim(),
                        Distance = form.Distance!.Value,
                        EntryFee = form.EntryFee!.Value
                    });

                    SetSuccess($"Category “{form.CategoryName.Trim()}” added.");

                    return RedirectToAction(nameof(Index), new { eventId = form.EventID });
                }
                catch (ApiException ex) when (IsRecoverable(ex))
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View("Index", new CategoriesPageViewModel
            {
                Event = ev,
                Form = form
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int eventId)
        {
            try
            {
                await Api.DeleteCategoryAsync(id);
                SetSuccess("Category removed.");
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                SetError(ex.Message);
            }

            return RedirectToAction(nameof(Index), new { eventId });
        }
    }
}
