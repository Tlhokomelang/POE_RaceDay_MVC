namespace POE_RaceDay_MVC.Models
{
    // These classes mirror the JSON returned by the Part 2 API.
    // They carry data only; all business rules live in the API.

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
        public ApiUser User { get; set; } = new();
    }

    public class ApiUser
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class ProfileResponse
    {
        public ApiUser Profile { get; set; } = new();
    }

    public class EventSummary
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int OrganiserID { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImage { get; set; }
        public int CategoryCount { get; set; }
    }

    public class OrganiserEvent : EventSummary
    {
        public int EnrolmentCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int ResultCount { get; set; }
    }

    public class EventDetail : EventSummary
    {
        public string OrganiserName { get; set; } = string.Empty;
        public List<CategoryInfo> Categories { get; set; } = new();
    }

    public class CategoryInfo
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
        public int EventID { get; set; }
        public int EnrolmentCount { get; set; }
    }

    public class MyEnrolment
    {
        public int EnrolmentID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string? BannerImage { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EventEnrolment
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string? ParticipantPhone { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ResultID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class MyResult
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int? ResultID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public int TotalFinishers { get; set; }
    }

    // ----- Request payloads sent to the API -----

    public class RegisterApiRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }

    public class EventApiRequest
    {
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    public class CategoryApiRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
        public int EventID { get; set; }
    }

    public class ResultApiRequest
    {
        public string FinishTime { get; set; } = string.Empty;
        public int Position { get; set; }
        public int EnrolmentID { get; set; }
    }

    public class EventQuery
    {
        public string? Search { get; set; }
        public string? EventType { get; set; }
        public string? Location { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MaxDistance { get; set; }
    }
}
