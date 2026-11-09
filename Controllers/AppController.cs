using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    // Shared behaviour for every controller.
    public abstract class AppController : Controller
    {
        protected void SetSuccess(string message) => TempData["Success"] = message;
        protected void SetError(string message) => TempData["Error"] = message;
        protected void SetWarning(string message) => TempData["Warning"] = message;

        // 401/403 are handled globally (log in again / access denied); every
        // other API failure is shown to the user next to the form they used.
        protected static bool IsRecoverable(ApiException ex) =>
            ex.StatusCode != 401 && ex.StatusCode != 403;

        protected IActionResult RedirectToRoleHome(ISessionUser user) =>
            user.IsOrganiser
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Home");
    }
}
