using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RoleAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string _role;

    public RoleAuthorizeAttribute(string role)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var role = session.GetString("Role");

        if (string.IsNullOrEmpty(role) || role != _role)
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
        }
    }
}
