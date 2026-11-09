using System.Globalization;
using System.Text.RegularExpressions;

namespace POE_RaceDay_MVC.Helpers
{
    // Presentation helpers only. No business rules live here.
    public static class UiHelpers
    {
        public static string Ordinal(int n)
        {
            int lastTwo = n % 100;

            if (lastTwo is >= 11 and <= 13) return n + "th";

            return (n % 10) switch
            {
                1 => n + "st",
                2 => n + "nd",
                3 => n + "rd",
                _ => n + "th"
            };
        }

        public static string StatusClass(string? status) => status switch
        {
            "Confirmed" => "status-confirmed",
            "Cancelled" => "status-cancelled",
            _ => "status-pending"
        };

        public static string Media(string? blobName) =>
            string.IsNullOrWhiteSpace(blobName) ? string.Empty : "/media/" + blobName;

        public static string Km(decimal distance) =>
            distance.ToString("0.##", CultureInfo.InvariantCulture) + " km";

        public static string Fee(decimal fee) =>
            fee == 0 ? "Free" : "R" + fee.ToString("0.00", CultureInfo.InvariantCulture);

        public static string Initials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";

            var parts = name.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return parts.Length == 1
                ? parts[0][..1].ToUpperInvariant()
                : (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }

        public static string FirstName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "there";

            return name.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public static string LongDate(DateTime date) =>
            date.ToString("ddd d MMM yyyy, HH:mm", CultureInfo.InvariantCulture);

        public static string ShortDate(DateTime date) =>
            date.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

        public static string DaysUntil(DateTime date)
        {
            int days = (int)Math.Ceiling((date - DateTime.UtcNow).TotalDays);

            return days switch
            {
                <= 0 => "Today",
                1 => "Tomorrow",
                _ => $"In {days} days"
            };
        }
    }

    // Quick client-side style checks so users get a friendly message
    // before an image is sent on to the API (which validates it again).
    public static class UploadValidator
    {
        private static readonly string[] Extensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public const long MaxBytes = 5 * 1024 * 1024;

        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            if (file.Length > MaxBytes)
                return "The image is too large. The maximum size is 5 MB.";

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Extensions.Contains(extension))
                return "Please choose a JPG, PNG, WEBP or GIF image.";

            return null;
        }
    }

    public static class FinishTimeParser
    {
        private static readonly Regex Pattern =
            new(@"^\d{1,2}:[0-5]\d:[0-5]\d$", RegexOptions.Compiled);

        // Returns hh:mm:ss, or null when the text is not a valid time.
        public static string? Normalise(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string text = input.Trim();

            if (!Pattern.IsMatch(text)) return null;

            if (!TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var time))
                return null;

            if (time >= TimeSpan.FromDays(1)) return null;

            return time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
        }
    }
}
