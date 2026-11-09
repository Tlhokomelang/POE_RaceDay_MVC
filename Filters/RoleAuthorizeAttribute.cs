using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Filters
{
    // Blocks unauthenticated visitors and users with the wrong role.
    // Authentication state comes from the server-side session.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.RequestServices
                .GetRequiredService<ISessionUser>();

            if (!user.IsAuthenticated)
            {
                var request = context.HttpContext.Request;

                string? returnUrl = HttpMethods.IsGet(request.Method)
                    ? request.Path + request.QueryString
                    : null;

                context.Result = new RedirectToActionResult(
                    "Login", "Account", new { returnUrl });

                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(user.Role))
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied", "Account", null);
            }
        }
    }
}
