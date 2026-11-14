namespace RaceDay.Web.Services
{
    public interface IUserSessionService
    {
        int? UserId { get; }
        string? Email { get; }
        string? FullName { get; }
        string? Role { get; }
        string? ProfilePictureUrl { get; }
        string? Token { get; }

        bool IsAuthenticated { get; }
        bool IsOrganiser { get; }
        bool IsParticipant { get; }

        void SetUserSession(
            int userId,
            string email,
            string fullName,
            string role,
            string? profilePictureUrl,
            string? token);

        void UpdateProfile(string fullName, string? profilePictureUrl);
        void ClearSession();
    }
}
