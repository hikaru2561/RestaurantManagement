using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using RestaurantManagement.Helpers;
using RestaurantManagement.Models;


namespace RestaurantManagement.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                return role switch
                {
                    "Admin" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                    "Staff" => RedirectToAction("Index", "Dashboard", new { area = "Staff" }),
                    "Customer" => RedirectToAction("Index", "Dashboard", new { area = "Customer" }),
                    _ => RedirectToAction("Login")
                };
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            string hashedPassword = HashHelper.ComputeSha256Hash(password);
            var user = _context.Accounts
                .FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("Role", user.Role)
    };

            // Gắn thêm claims nếu là Staff
            if (user.Role == "Staff")
            {
                var staff = _context.Staffs.FirstOrDefault(s => s.Username == user.Username);
                if (staff != null)
                {
                    claims.Add(new Claim("StaffId", staff.StaffId.ToString()));
                    claims.Add(new Claim("FullName", staff.Name ?? ""));
                    claims.Add(new Claim("Phone", staff.Phone ?? ""));
                }
            }

            // Gắn thêm claims nếu là Customer
            if (user.Role == "Customer")
            {
                var customer = _context.Customers.FirstOrDefault(c => c.Username == user.Username);
                if (customer != null)
                {
                    claims.Add(new Claim("CustomerId", customer.CustomerId.ToString()));
                    claims.Add(new Claim("FullName", customer.Name ?? ""));
                    claims.Add(new Claim("Phone", customer.Phone ?? ""));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            // Nếu có ReturnUrl
            var returnUrl = Request.Query["ReturnUrl"].ToString();
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Chuyển hướng theo Role
            return user.Role switch
            {
                "Admin" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                "Staff" => RedirectToAction("Index", "Dashboard", new { area = "Staff" }),
                "Customer" => RedirectToAction("Index", "Dashboard", new { area = "Customer" }),
                _ => RedirectToAction("Login")
            };
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string username, string password, string phone, string email)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin bắt buộc.";
                return View();
            }

            if (_context.Accounts.Any(a => a.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            string hashedPassword = HashHelper.ComputeSha256Hash(password);

            var account = new Account
            {
                Username = username,
                Password = hashedPassword,
                Role = "Customer"
            };
            _context.Accounts.Add(account);
            _context.SaveChanges();

            var customer = new Customer
            {
                Username = username,
                Name = name,
                Phone = phone,
                Email = email,
                IsVIP = false
            };
            _context.Customers.Add(customer);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký tài khoản thành công! Hãy đăng nhập.";
            return RedirectToAction("Login");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete(".AspNetCore.CustomAuthCookie");
            return RedirectToAction("Login", "Login");
        }

        public IActionResult Denied()
        {
            return View("~/Views/Shared/Denied.cshtml"); // Hoặc tạo Denied trong Views/Login
        }
    }
}
