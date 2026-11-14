using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public const string KeyUserId = "Auth_UserId";
        public const string KeyEmail = "Auth_Email";
        public const string KeyFullName = "Auth_FullName";
        public const string KeyRole = "Auth_Role";
        public const string KeyProfilePic = "Auth_ProfilePic";
        public const string KeyToken = "Auth_Token";

        public UserSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated => UserId.HasValue && UserId.Value > 0;

        public int? UserId => Session?.GetInt32(KeyUserId);

        public string? Email => Session?.GetString(KeyEmail);

        public string? FullName => Session?.GetString(KeyFullName);

        public string? Role => Session?.GetString(KeyRole);

        public string? ProfilePictureUrl => Session?.GetString(KeyProfilePic);

        public string? Token => Session?.GetString(KeyToken);

        public bool IsOrganiser => string.Equals(Role, "Organiser", StringComparison.OrdinalIgnoreCase);

        public bool IsParticipant => string.Equals(Role, "Participant", StringComparison.OrdinalIgnoreCase);

        public void SetUserSession(int userId, string email, string fullName, string role, string? profilePictureUrl, string? token)
        {
            if (Session == null) return;
            Session.SetInt32(KeyUserId, userId);
            Session.SetString(KeyEmail, email);
            Session.SetString(KeyFullName, fullName);
            Session.SetString(KeyRole, role);
            if (!string.IsNullOrEmpty(profilePictureUrl))
                Session.SetString(KeyProfilePic, profilePictureUrl);
            else
                Session.Remove(KeyProfilePic);

            if (!string.IsNullOrEmpty(token))
                Session.SetString(KeyToken, token);
        }

        public void UpdateProfile(string fullName, string? profilePictureUrl)
        {
            if (Session == null) return;
            Session.SetString(KeyFullName, fullName);
            if (!string.IsNullOrEmpty(profilePictureUrl))
                Session.SetString(KeyProfilePic, profilePictureUrl);
        }

        public void ClearSession()
        {
            Session?.Clear();
        }
    }
}
