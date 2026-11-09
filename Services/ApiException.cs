namespace POE_RaceDay_MVC.Services
{
    // A failure talking to the RaceDay API. The message is always safe to
    // show to a user; raw server errors are never passed through.
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public IReadOnlyList<string> Errors { get; }

        public ApiException(
            int statusCode, string message, IEnumerable<string>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = (errors ?? Array.Empty<string>()).ToList();
        }
    }
}
