using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiniERPsystem.Models;
using MiniERPsystem.Data;

namespace MiniERPsystem.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var email = context.HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null
                );
                return;
            }

            base.OnActionExecuting(context);
        }
        protected void LogActivity(string action)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
                return;

            var log = new ActivityLog
            {
                UserEmail = email,
                Action = action,
                CreatedAt = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            };

            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }

    }
}
        
    