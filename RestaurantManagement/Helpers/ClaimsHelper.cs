using System.Security.Claims;

namespace RestaurantManagement.Helpers
{
    public static class ClaimsHelper
    {
        public static int? GetStaffId(ClaimsPrincipal user)
        {
            var val = user.FindFirst("StaffId")?.Value;
            return int.TryParse(val, out var id) ? id : null;
        }

        public static int? GetCustomerId(ClaimsPrincipal user)
        {
            var val = user.FindFirst("CustomerId")?.Value;
            return int.TryParse(val, out var id) ? id : null;
        }

        public static string? GetFullName(ClaimsPrincipal user)
        {
            return user.FindFirst("FullName")?.Value;
        }

        public static string? GetPhone(ClaimsPrincipal user)
        {
            return user.FindFirst("Phone")?.Value;
        }

        public static string? GetRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static string? GetUsername(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
