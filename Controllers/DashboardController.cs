using Microsoft.AspNetCore.Mvc;
using MiniERPsystem.Data;
using MiniERPsystem.Services;
using System;
using System.Linq;

namespace MiniERPsystem.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly AiInsightsService _insights;

        public DashboardController(ApplicationDbContext context, AiInsightsService insights)
            : base(context)
        {
            _insights = insights;
        }

        public IActionResult Index()
        {
            var totalSales =
                _context.Sales.Sum(s => (decimal?)s.TotalAmount) ?? 0;

            DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);

            var monthlyRevenue =
                _context.Sales
                    .Where(s => s.SaleDate >= startOfMonth && s.SaleDate < startOfNextMonth)
                    .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.TotalSales = totalSales.ToString("N0");
            ViewBag.MonthlyRevenue = monthlyRevenue.ToString("N0");
            ViewBag.TotalSalesCount = _context.Sales.Count();
            ViewBag.LowStockCount = _context.Products.Count(p => p.StockQuantity <= 5);
            ViewBag.TotalCustomers = _context.Customers.Count();
            ViewBag.TotalProducts = _context.Products.Count(p => p.IsActive);

            ViewBag.SystemMessage =
                ViewBag.LowStockCount > 0
                    ? $"{ViewBag.LowStockCount} product(s) are running low on stock."
                    : "Inventory levels are healthy.";

            ViewBag.RecentSales = _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToList();

            return View();
        }

        public IActionResult Insights()
        {
            var data = _insights.Generate();
            return View(data);
        }


    }
}
