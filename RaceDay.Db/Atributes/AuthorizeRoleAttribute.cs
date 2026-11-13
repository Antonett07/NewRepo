using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.DB.Atributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authentication required. Please log in." });
                return;
            }

            if (_allowedRoles.Length > 0)
            {
                var userRole = context.HttpContext.Session.GetString("UserRole");
                if (userRole == null || !_allowedRoles.Contains(userRole))
                {
                    context.Result = new ObjectResult(new { message = "Access forbidden for your user role." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
        }

    }
}
