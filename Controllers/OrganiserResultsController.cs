using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Helpers;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    [RoleAuthorize("Organiser")]
    public class OrganiserResultsController : OrganiserControllerBase
    {
        public OrganiserResultsController(IRaceDayApi api, ISessionUser user)
            : base(api, user)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? eventId)
        {
            if (eventId == null)
            {
                return View("Picker", await BuildPickerAsync(
                    "Results",
                    "Choose an event to capture and publish finish times and positions.",
                    "OrganiserResults",
                    "Enter results"));
            }

            EventDetail? ev = await LoadOwnedEventAsync(eventId.Value);

            if (ev == null) return RedirectToAction("AccessDenied", "Account");

            // Cancelled enrolments never race.
            var enrolments = (await Api.GetEventEnrolmentsAsync(eventId.Value))
                .Where(e => e.Status != "Cancelled")
                .ToList();

            return View(new ResultsPageViewModel
            {
                Event = ev,
                Enrolments = enrolments,
                Rows = enrolments.Select(e => new ResultRowInput
                {
                    EnrolmentID = e.EnrolmentID,
                    ResultID = e.ResultID,
                    FinishTime = e.FinishTime,
                    Position = e.Position
                }).ToList()
            });
        }

        // Publishes every row the organiser filled in. Rows left blank are
        // ignored, so results can be entered in several sittings.
        [HttpPost]
        public async Task<IActionResult> Save(int eventId, List<ResultRowInput>? rows)
        {
            EventDetail? ev = await LoadOwnedEventAsync(eventId);

            if (ev == null) return RedirectToAction("AccessDenied", "Account");

            var enrolments = await Api.GetEventEnrolmentsAsync(eventId);

            int saved = 0;
            var problems = new List<string>();

            foreach (ResultRowInput row in rows ?? new List<ResultRowInput>())
            {
                EventEnrolment? enrolment = enrolments
                    .FirstOrDefault(e => e.EnrolmentID == row.EnrolmentID);

                if (enrolment == null) continue;

                bool hasTime = !string.IsNullOrWhiteSpace(row.FinishTime);
                bool hasPosition = row.Position.HasValue;

                if (!hasTime && !hasPosition) continue;

                string name = enrolment.ParticipantName;

                if (hasTime != hasPosition)
                {
                    problems.Add($"{name}: enter both a finish time and a position.");
                    continue;
                }

                string? time = FinishTimeParser.Normalise(row.FinishTime);

                if (time == null)
                {
                    problems.Add($"{name}: finish time must look like 00:28:35 (hh:mm:ss).");
                    continue;
                }

                if (row.Position!.Value <= 0)
                {
                    problems.Add($"{name}: position must be 1 or higher.");
                    continue;
                }

                // Nothing changed since it was last published.
                if (enrolment.ResultID.HasValue &&
                    enrolment.FinishTime == time &&
                    enrolment.Position == row.Position)
                {
                    continue;
                }

                var request = new ResultApiRequest
                {
                    EnrolmentID = enrolment.EnrolmentID,
                    FinishTime = time,
                    Position = row.Position.Value
                };

                try
                {
                    if (enrolment.ResultID.HasValue)
                        await Api.UpdateResultAsync(enrolment.ResultID.Value, request);
                    else
                        await Api.CreateResultAsync(request);

                    saved++;
                }
                catch (ApiException ex) when (IsRecoverable(ex))
                {
                    problems.Add($"{name}: {ex.Message}");
                }
            }

            if (problems.Count > 0)
            {
                string prefix = saved > 0 ? $"{saved} result(s) published. " : string.Empty;

                SetError(prefix + "Some rows need attention:\n" +
                    string.Join("\n", problems));
            }
            else if (saved > 0)
            {
                SetSuccess($"{saved} result(s) published. Participants can now see them under My Results.");
            }
            else
            {
                SetWarning("There were no new results to publish.");
            }

            return RedirectToAction(nameof(Index), new { eventId });
        }
    }
}
