using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RestaurantManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult About() => View();

        public IActionResult Contact() => View();

        public IActionResult RedirectToCustomerDashboard()
        {
            // Nếu chưa đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Login");
            }

            // Nếu đã đăng nhập nhưng không phải là Customer
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Customer")
            {
                return RedirectToAction("Login", "Login");
            }

            // Đúng role Customer → chuyển sang giao diện Dashboard khách hàng
            return RedirectToAction("Index", "Dashboard", new { area = "Customer" });
        }
    }
}
