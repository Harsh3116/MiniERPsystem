using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniERPsystem.Models;
using MiniERPsystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MiniERPsystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var totalSalesAmount = _context.Sales.Sum(s => (decimal?)s.TotalAmount) ?? 0;
            var totalSalesCount = _context.Sales.Count();

            var today = DateTime.Today;
            var todaysSalesAmount = _context.Sales
                .Where(s => s.SaleDate.Date == today)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var lowStockProducts = _context.Products
                .Where(p => p.StockQuantity <= 5)
                .ToList();

            ViewBag.TotalSalesAmount = totalSalesAmount;
            ViewBag.TotalSalesCount = totalSalesCount;
            ViewBag.TodaysSalesAmount = todaysSalesAmount;
            ViewBag.LowStockProducts = lowStockProducts;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}
