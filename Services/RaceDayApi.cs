using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using POE_RaceDay_MVC.Models;

namespace POE_RaceDay_MVC.Services
{
    // Every piece of data in the MVC application comes through this
    // client. There is no database access and no direct Azure access.
    public interface IRaceDayApi
    {
        // Auth
        Task<LoginResponse> LoginAsync(string email, string password);
        Task RegisterAsync(RegisterApiRequest request);

        // Events
        Task<List<EventSummary>> GetEventsAsync(EventQuery? query = null);
        Task<List<string>> GetEventTypesAsync();
        Task<EventDetail> GetEventAsync(int id);
        Task<List<OrganiserEvent>> GetMyEventsAsync();
        Task<int> CreateEventAsync(EventApiRequest request);
        Task UpdateEventAsync(int id, EventApiRequest request);
        Task DeleteEventAsync(int id);
        Task UploadBannerAsync(int eventId, IFormFile file);

        // Categories
        Task CreateCategoryAsync(CategoryApiRequest request);
        Task DeleteCategoryAsync(int id);

        // Enrolments
        Task CreateEnrolmentAsync(int categoryId);
        Task<List<MyEnrolment>> GetMyEnrolmentsAsync();
        Task WithdrawEnrolmentAsync(int enrolmentId);
        Task<List<EventEnrolment>> GetEventEnrolmentsAsync(int eventId);
        Task SetEnrolmentStatusAsync(int enrolmentId, string status);

        // Results
        Task<List<MyResult>> GetMyResultsAsync();
        Task CreateResultAsync(ResultApiRequest request);
        Task UpdateResultAsync(int resultId, ResultApiRequest request);

        // Profile
        Task<ApiUser> GetProfileAsync();
        Task UpdateProfileAsync(string fullName, string? phoneNumber);
        Task<string?> UploadProfilePictureAsync(IFormFile file);

        // Files (served from Azure Blob Storage by the API)
        Task<(byte[] Content, string ContentType)?> GetFileAsync(string blobName);
    }

    public class RaceDayApi : IRaceDayApi
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _http;
        private readonly ISessionUser _session;
        private readonly ILogger<RaceDayApi> _logger;

        public RaceDayApi(
            HttpClient http, ISessionUser session, ILogger<RaceDayApi> logger)
        {
            _http = http;
            _session = session;
            _logger = logger;
        }

        // ---------- Auth ----------

        public Task<LoginResponse> LoginAsync(string email, string password) =>
            SendAsync<LoginResponse>(
                HttpMethod.Post, "api/Auth/login", new { email, password });

        public Task RegisterAsync(RegisterApiRequest request) =>
            SendAsync(HttpMethod.Post, "api/Auth/register", request);

        // ---------- Events ----------

        public Task<List<EventSummary>> GetEventsAsync(EventQuery? query = null)
        {
            var parts = new List<string>();

            void Add(string key, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    parts.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
            }

            if (query != null)
            {
                Add("search", query.Search);
                Add("eventType", query.EventType);
                Add("location", query.Location);
                Add("fromDate", query.FromDate?.ToString("yyyy-MM-dd"));
                Add("toDate", query.ToDate?.ToString("yyyy-MM-dd"));
                Add("maxDistance", query.MaxDistance?.ToString(
                    System.Globalization.CultureInfo.InvariantCulture));
            }

            string url = "api/events" +
                (parts.Count > 0 ? "?" + string.Join("&", parts) : "");

            return SendAsync<List<EventSummary>>(HttpMethod.Get, url);
        }

        public Task<List<string>> GetEventTypesAsync() =>
            SendAsync<List<string>>(HttpMethod.Get, "api/events/types");

        public Task<EventDetail> GetEventAsync(int id) =>
            SendAsync<EventDetail>(HttpMethod.Get, $"api/events/{id}");

        public Task<List<OrganiserEvent>> GetMyEventsAsync() =>
            SendAsync<List<OrganiserEvent>>(HttpMethod.Get, "api/events/mine");

        public async Task<int> CreateEventAsync(EventApiRequest request)
        {
            using HttpResponseMessage response = await SendRawAsync(
                BuildRequest(HttpMethod.Post, "api/events", request));

            using JsonDocument doc = await ParseDocumentAsync(response);

            return doc.RootElement.TryGetProperty("eventId", out var id)
                ? id.GetInt32()
                : 0;
        }

        public Task UpdateEventAsync(int id, EventApiRequest request) =>
            SendAsync(HttpMethod.Put, $"api/events/{id}", request);

        public Task DeleteEventAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/events/{id}");

        public async Task UploadBannerAsync(int eventId, IFormFile file)
        {
            using JsonDocument uploadResult = await UploadAsync(
                $"api/events/{eventId}/banner", file);
        }

        // ---------- Categories ----------

        public Task CreateCategoryAsync(CategoryApiRequest request) =>
            SendAsync(HttpMethod.Post, "api/category", request);

        public Task DeleteCategoryAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/category/{id}");

        // ---------- Enrolments ----------

        public Task CreateEnrolmentAsync(int categoryId) =>
            SendAsync(HttpMethod.Post, "api/enrolment",
                new { categoryID = categoryId });

        public Task<List<MyEnrolment>> GetMyEnrolmentsAsync() =>
            SendAsync<List<MyEnrolment>>(HttpMethod.Get, "api/enrolment/mine");

        public Task WithdrawEnrolmentAsync(int enrolmentId) =>
            SendAsync(HttpMethod.Delete, $"api/enrolment/{enrolmentId}");

        public Task<List<EventEnrolment>> GetEventEnrolmentsAsync(int eventId) =>
            SendAsync<List<EventEnrolment>>(
                HttpMethod.Get, $"api/enrolment/event/{eventId}");

        public Task SetEnrolmentStatusAsync(int enrolmentId, string status) =>
            SendAsync(HttpMethod.Put, $"api/enrolment/{enrolmentId}/status",
                new { status });

        // ---------- Results ----------

        public Task<List<MyResult>> GetMyResultsAsync() =>
            SendAsync<List<MyResult>>(HttpMethod.Get, "api/Result/mine");

        public Task CreateResultAsync(ResultApiRequest request) =>
            SendAsync(HttpMethod.Post, "api/Result", request);

        public Task UpdateResultAsync(int resultId, ResultApiRequest request) =>
            SendAsync(HttpMethod.Put, $"api/Result/{resultId}", request);

        // ---------- Profile ----------

        public async Task<ApiUser> GetProfileAsync()
        {
            var response = await SendAsync<ProfileResponse>(
                HttpMethod.Get, "api/profile");

            return response.Profile;
        }

        public Task UpdateProfileAsync(string fullName, string? phoneNumber) =>
            SendAsync(HttpMethod.Put, "api/profile",
                new { fullName, phoneNumber });

        public async Task<string?> UploadProfilePictureAsync(IFormFile file)
        {
            using JsonDocument doc = await UploadAsync(
                "api/profile/picture", file);

            return doc.RootElement.TryGetProperty("profilePicture", out var p)
                ? p.GetString()
                : null;
        }

        // ---------- Files ----------

        public async Task<(byte[] Content, string ContentType)?> GetFileAsync(
            string blobName)
        {
            try
            {
                using HttpResponseMessage response =
                    await _http.GetAsync("api/files/" + blobName);

                if (!response.IsSuccessStatusCode) return null;

                byte[] bytes = await response.Content.ReadAsByteArrayAsync();

                string type = response.Content.Headers.ContentType?.MediaType
                    ?? "application/octet-stream";

                return (bytes, type);
            }
            catch (Exception ex) when (
                ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "File request failed.");
                return null;
            }
        }

        // ---------- plumbing ----------

        private async Task<T> SendAsync<T>(
            HttpMethod method, string url, object? body = null)
        {
            using HttpResponseMessage response = await SendRawAsync(
                BuildRequest(method, url, body));

            string json = await response.Content.ReadAsStringAsync();

            try
            {
                T? value = JsonSerializer.Deserialize<T>(json, JsonOptions);

                if (value == null) throw new JsonException("Empty response.");

                return value;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unexpected API response for {Url}.", url);

                throw new ApiException(502,
                    "We received an unexpected response from the RaceDay service.");
            }
        }

        private async Task SendAsync(
            HttpMethod method, string url, object? body = null)
        {
            using HttpResponseMessage response = await SendRawAsync(
                BuildRequest(method, url, body));
        }

        private async Task<JsonDocument> UploadAsync(string url, IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            await using Stream stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType);

            // The form field name must match the API parameter: "file".
            content.Add(fileContent, "file", Path.GetFileName(file.FileName));

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            using HttpResponseMessage response = await SendRawAsync(request);

            return await ParseDocumentAsync(response);
        }

        private static async Task<JsonDocument> ParseDocumentAsync(
            HttpResponseMessage response)
        {
            string json = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonDocument.Parse(
                    string.IsNullOrWhiteSpace(json) ? "{}" : json);
            }
            catch (JsonException)
            {
                return JsonDocument.Parse("{}");
            }
        }

        private static HttpRequestMessage BuildRequest(
            HttpMethod method, string url, object? body)
        {
            var request = new HttpRequestMessage(method, url);

            if (body != null)
            {
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body, JsonOptions),
                    Encoding.UTF8,
                    "application/json");
            }

            return request;
        }

        private async Task<HttpResponseMessage> SendRawAsync(
            HttpRequestMessage request)
        {
            string? token = _session.Token;

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response;

            try
            {
                response = await _http.SendAsync(request);
            }
            catch (Exception ex) when (
                ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "RaceDay API is unreachable.");

                throw new ApiException(503,
                    "The RaceDay service is currently unreachable. Please try again shortly.");
            }

            if (!response.IsSuccessStatusCode)
            {
                ApiException error = await BuildExceptionAsync(response);
                response.Dispose();
                throw error;
            }

            return response;
        }

        private static async Task<ApiException> BuildExceptionAsync(
            HttpResponseMessage response)
        {
            int code = (int)response.StatusCode;
            string body = await response.Content.ReadAsStringAsync();

            string? message = null;
            var errors = new List<string>();

            try
            {
                using JsonDocument doc = JsonDocument.Parse(body);
                JsonElement root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var m) &&
                        m.ValueKind == JsonValueKind.String)
                    {
                        message = m.GetString();
                    }

                    // [ApiController] validation failures
                    if (root.TryGetProperty("errors", out var e) &&
                        e.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in e.EnumerateObject())
                        {
                            if (property.Value.ValueKind != JsonValueKind.Array)
                                continue;

                            foreach (var item in property.Value.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String)
                                    errors.Add(item.GetString()!);
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON: fall through to the friendly defaults below.
            }

            if (message == null && errors.Count > 0) message = errors[0];

            // Never surface raw server-side detail to the user.
            if (code >= 500 || string.IsNullOrWhiteSpace(message))
            {
                message = code switch
                {
                    401 => "Your session has expired. Please log in again.",
                    403 => "You do not have permission to do that.",
                    404 => "We couldn't find what you were looking for.",
                    >= 500 => "Something went wrong on our side. Please try again shortly.",
                    _ => "That request could not be completed. Please check your details and try again."
                };
            }

            return new ApiException(code, message!, errors);
        }
    }
}
