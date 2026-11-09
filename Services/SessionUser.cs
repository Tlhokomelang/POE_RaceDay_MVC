using POE_RaceDay_MVC.Models;

namespace POE_RaceDay_MVC.Services
{
    // The signed-in user, held in server-side session state so the login
    // and role persist across every page.
    public interface ISessionUser
    {
        bool IsAuthenticated { get; }
        string? Token { get; }
        int UserId { get; }
        string Name { get; }
        string Email { get; }
        string Role { get; }
        string? ProfilePicture { get; set; }
        bool IsOrganiser { get; }
        bool IsParticipant { get; }

        void SignIn(LoginResponse login);
        void UpdateName(string name);
        void SignOut();
    }

    public class SessionUser : ISessionUser
    {
        private const string TokenKey = "rd.token";
        private const string IdKey = "rd.uid";
        private const string NameKey = "rd.name";
        private const string EmailKey = "rd.email";
        private const string RoleKey = "rd.role";
        private const string PictureKey = "rd.pic";

        private readonly IHttpContextAccessor _accessor;

        public SessionUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ISession? Session => _accessor.HttpContext?.Session;

        public string? Token => Session?.GetString(TokenKey);

        public bool IsAuthenticated =>
            !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(Role);

        public int UserId => Session?.GetInt32(IdKey) ?? 0;

        public string Name => Session?.GetString(NameKey) ?? string.Empty;

        public string Email => Session?.GetString(EmailKey) ?? string.Empty;

        public string Role => Session?.GetString(RoleKey) ?? string.Empty;

        public string? ProfilePicture
        {
            get => Session?.GetString(PictureKey);
            set
            {
                if (Session == null) return;

                if (string.IsNullOrEmpty(value))
                    Session.Remove(PictureKey);
                else
                    Session.SetString(PictureKey, value);
            }
        }

        public bool IsOrganiser => IsAuthenticated && Role == "Organiser";

        public bool IsParticipant => IsAuthenticated && Role == "Participant";

        public void SignIn(LoginResponse login)
        {
            if (Session == null) return;

            Session.Clear();
            Session.SetString(TokenKey, login.Token);
            Session.SetInt32(IdKey, login.User.UserID);
            Session.SetString(NameKey, login.User.FullName);
            Session.SetString(EmailKey, login.User.Email);
            Session.SetString(RoleKey, login.User.Role);
        }

        public void UpdateName(string name)
        {
            Session?.SetString(NameKey, name);
        }

        public void SignOut()
        {
            Session?.Clear();
        }
    }
}
