using Microsoft.AspNetCore.Mvc;
using MiniERPsystem.Models;
using MiniERPsystem.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
namespace MiniERPsystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (Request.Cookies["RememberUser"] != null)
            {
                ViewBag.RememberedUser = Request.Cookies["RememberUser"];
            }
            return View();
        }



        [HttpPost]
        public IActionResult Login(string email, string password, string role, bool rememberMe)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == email &&
                u.Password == password &&
                u.Role == role
            );

            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserEmail", user.Email);
                LogActivity("Logged in");

                if (rememberMe)
                {
                    Response.Cookies.Append(
                        "RememberUser",
                        user.Email,
                        new CookieOptions { Expires = DateTime.Now.AddDays(7) }
                    );
                }

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid email, password, or role";
            return View();
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(User user)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied");
            }

            // 1️⃣ Basic validation
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                ViewBag.Error = "Email and Password are required.";
                return View();
            }

            // 2️⃣ Check if email already exists
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == user.Email);

            if (existingUser != null)
            {
                ViewBag.Error = "User with this email already exists.";
                return View();
            }

            // 3️⃣ Save user
            _context.Users.Add(user);
            _context.SaveChanges();

            ViewBag.Success = "User created successfully!";
            return View();
        }

        public IActionResult Logout()
        {
            LogActivity("Logged out");
            
            HttpContext.Session.Clear();

            // 2️⃣ Remove remember-me cookie (if exists)
            if (Request.Cookies["RememberUser"] != null)
            {
                Response.Cookies.Delete("RememberUser");
            }
            
            // 3️⃣ Redirect to Login page
            return RedirectToAction("Login", "Account");
        }
        public IActionResult Users()
        {
            // Admin check
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied");
            }

            var users = _context.Users.ToList();
            return View(users);
        }
        public IActionResult EditUser(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(User updatedUser)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == updatedUser.Id);

            if (user == null)
            {
                return NotFound();
            }

            user.Role = updatedUser.Role;
            _context.SaveChanges();

            return RedirectToAction("Users");
        }
        public IActionResult ActivityLogs()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied");
            }

            var logs = _context.ActivityLogs
                .OrderByDescending(l => l.CreatedAt)
                .ToList();

            return View(logs);
        }
        private void LogActivity(string action)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
                return;

            var log = new ActivityLog
            {
                UserEmail = email,
                Action = action,
                CreatedAt = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }


        public IActionResult UserHome()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
