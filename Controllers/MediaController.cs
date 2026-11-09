using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    // Serves event banners and profile pictures to the browser. The bytes
    // come from the API (which reads Azure Blob Storage), so the browser
    // and this app never touch Azure directly.
    public class MediaController : Controller
    {
        private static readonly Regex SafeName =
            new(@"^[A-Za-z0-9/_\-.]+$", RegexOptions.Compiled);

        private readonly IRaceDayApi _api;

        public MediaController(IRaceDayApi api)
        {
            _api = api;
        }

        [HttpGet("/media/{**name}")]
        public async Task<IActionResult> Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                name.Contains("..") ||
                !SafeName.IsMatch(name))
            {
                return NotFound();
            }

            var file = await _api.GetFileAsync(name);

            if (file == null || !file.Value.ContentType.StartsWith("image/"))
            {
                return NotFound();
            }

            Response.Headers.CacheControl = "public,max-age=3600";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return File(file.Value.Content, file.Value.ContentType);
        }
    }
}
