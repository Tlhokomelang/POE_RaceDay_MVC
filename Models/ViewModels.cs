using System.ComponentModel.DataAnnotations;

namespace POE_RaceDay_MVC.Models
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; } = 500;
        public string Title { get; set; } = "Something went off course";
        public string Message { get; set; } =
            "An unexpected problem occurred. Please try again.";
    }

    // ---------- Account ----------

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "That doesn't look like a valid email address.")]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        [StringLength(72)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "Your name must be between 2 and 150 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "That doesn't look like a valid email address.")]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [RegularExpression(@"^[0-9+()\-\s]*$",
            ErrorMessage = "Use digits, spaces, + and - only.")]
        [Display(Name = "Phone number (optional)")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please choose a password.")]
        [StringLength(72, MinimumLength = 8,
            ErrorMessage = "Your password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose how you will use RaceDay.")]
        [RegularExpression("^(Participant|Organiser)$",
            ErrorMessage = "Please choose Participant or Organiser.")]
        public string Role { get; set; } = "Participant";
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "Your name must be between 2 and 150 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        [RegularExpression(@"^[0-9+()\-\s]*$",
            ErrorMessage = "Use digits, spaces, + and - only.")]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        // Read-only details
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
        public string? ProfilePicture { get; set; }
    }

    // ---------- Participant ----------

    public class EventListViewModel
    {
        public EventQuery Filter { get; set; } = new();
        public List<EventSummary> Events { get; set; } = new();
        public List<string> EventTypes { get; set; } = new();
        public bool HasFilter =>
            !string.IsNullOrWhiteSpace(Filter.Search) ||
            !string.IsNullOrWhiteSpace(Filter.EventType) ||
            !string.IsNullOrWhiteSpace(Filter.Location) ||
            Filter.FromDate.HasValue ||
            Filter.ToDate.HasValue ||
            Filter.MaxDistance.HasValue;
    }

    public class EventDetailsViewModel
    {
        public EventDetail Event { get; set; } = new();
        public MyEnrolment? ExistingEnrolment { get; set; }
        public bool IsPast => Event.EventDate <= DateTime.UtcNow;
    }

    public class LandingViewModel
    {
        public List<EventSummary> NextEvents { get; set; } = new();
    }

    // ---------- Organiser ----------

    public class DashboardViewModel
    {
        public List<OrganiserEvent> Events { get; set; } = new();

        public List<OrganiserEvent> Upcoming => Events
            .Where(e => e.EventDate >= DateTime.UtcNow)
            .OrderBy(e => e.EventDate)
            .ToList();

        public int TotalEnrolments => Events.Sum(e => e.EnrolmentCount);
        public int TotalPending => Events.Sum(e => e.PendingCount);
    }

    public class EventPickerViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string ButtonText { get; set; } = "Open";
        public List<OrganiserEvent> Events { get; set; } = new();
    }

    public class EventFormViewModel
    {
        public int? EventID { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(150, MinimumLength = 3,
            ErrorMessage = "The event name must be between 3 and 150 characters.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = string.Empty;

        [StringLength(1000,
            ErrorMessage = "The description can be at most 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose the event date and start time.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date and start time")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(250, ErrorMessage = "The location is too long.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the event distance in kilometres.")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = string.Empty;

        [Display(Name = "Banner image")]
        public IFormFile? Banner { get; set; }

        public string? CurrentBanner { get; set; }
        public List<string> EventTypes { get; set; } = new();
        public bool IsEdit => EventID.HasValue;
    }

    public class CategoryFormViewModel
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Please name the category.")]
        [StringLength(100, ErrorMessage = "The name is too long.")]
        [Display(Name = "Category name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the distance in kilometres.")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Please enter the entry fee (use 0 for free).")]
        [Range(0, 100000, ErrorMessage = "The entry fee cannot be negative.")]
        [Display(Name = "Entry fee (R)")]
        public decimal? EntryFee { get; set; }
    }

    public class CategoriesPageViewModel
    {
        public EventDetail Event { get; set; } = new();
        public CategoryFormViewModel Form { get; set; } = new();
    }

    public class EnrolmentsPageViewModel
    {
        public EventDetail Event { get; set; } = new();
        public List<EventEnrolment> Enrolments { get; set; } = new();
    }

    public class ResultRowInput
    {
        public int EnrolmentID { get; set; }
        public int? ResultID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class ResultsPageViewModel
    {
        public EventDetail Event { get; set; } = new();
        public List<EventEnrolment> Enrolments { get; set; } = new();
        public List<ResultRowInput> Rows { get; set; } = new();
        public bool EventHasConcluded => Event.EventDate <= DateTime.UtcNow;
    }
}
