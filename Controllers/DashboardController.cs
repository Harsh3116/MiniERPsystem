using Microsoft.AspNetCore.Mvc;
using MiniERPsystem.Data;
using System;
using System.Linq;

namespace MiniERPsystem.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(ApplicationDbContext context)
            :base(context)
        {
        }

        public IActionResult Index()
        {
            // TOTAL SALES (SAFE)
            ViewBag.TotalSales =
                _context.Sales.Sum(s => (decimal?)s.TotalAmount) ?? 0;

            // MONTHLY REVENUE (SAFE DATE RANGE)
            DateTime startOfMonth =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);

            ViewBag.MonthlyRevenue =
                _context.Sales
                    .Where(s => s.SaleDate >= startOfMonth &&
                                s.SaleDate < startOfNextMonth)
                    .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            // LOW STOCK COUNT
            ViewBag.LowStockCount =
                _context.Products.Count(p => p.StockQuantity <= 5);

            // TOTAL CUSTOMERS
            ViewBag.TotalCustomers =
                _context.Customers.Count();

            // SYSTEM MESSAGE
            ViewBag.SystemMessage =
                    ViewBag.LowStockCount > 0
        ? $"⚠️ {ViewBag.LowStockCount} products are low in stock."
        : "✅ Inventory levels are healthy.";

            return View();
        }


    }
}
