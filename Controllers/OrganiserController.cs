using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    [RoleAuthorize("Organiser")]
    public class OrganiserController : OrganiserControllerBase
    {
        public OrganiserController(IRaceDayApi api, ISessionUser user)
            : base(api, user)
        {
        }

        // Summary of every event the organiser has created
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            return View(new DashboardViewModel
            {
                Events = await Api.GetMyEventsAsync()
            });
        }
    }
}
