using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    public class HomeController : AppController
    {
        private readonly IRaceDayApi _api;
        private readonly ISessionUser _user;

        public HomeController(IRaceDayApi api, ISessionUser user)
        {
            _api = api;
            _user = user;
        }

        // Each visitor type gets its own home:
        //   guest       -> marketing landing page
        //   organiser   -> dashboard
        //   participant -> upcoming events with filters
        [HttpGet]
        public async Task<IActionResult> Index(EventQuery? filter)
        {
            if (!_user.IsAuthenticated)
            {
                var landing = new LandingViewModel();

                try
                {
                    var events = await _api.GetEventsAsync();
                    landing.NextEvents = events.Take(4).ToList();
                }
                catch (ApiException)
                {
                    // The landing page still works without the preview.
                }

                return View("Landing", landing);
            }

            if (_user.IsOrganiser)
                return RedirectToAction("Dashboard", "Organiser");

            filter ??= new EventQuery();

            var model = new EventListViewModel
            {
                Filter = filter,
                Events = await _api.GetEventsAsync(filter),
                EventTypes = await _api.GetEventTypesAsync()
            };

            return View(model);
        }

        [IgnoreAntiforgeryToken]
        public IActionResult Error(int? code)
        {
            int status = code ?? 500;

            var model = status switch
            {
                404 => new ErrorViewModel
                {
                    StatusCode = 404,
                    Title = "We couldn't find that page",
                    Message = "The page you're looking for has moved or doesn't exist."
                },
                403 => new ErrorViewModel
                {
                    StatusCode = 403,
                    Title = "That's off limits",
                    Message = "You don't have permission to view this page."
                },
                _ => new ErrorViewModel
                {
                    StatusCode = status,
                    Title = "Something went off course",
                    Message = "An unexpected problem occurred. Please try again in a moment."
                }
            };

            Response.StatusCode = status;

            return View(model);
        }
    }
}
