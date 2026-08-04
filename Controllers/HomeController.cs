using Microsoft.AspNetCore.Mvc;
using MiniERPsystem.Data;

namespace MiniERPsystem.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ApplicationDbContext context) : base(context) { }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
