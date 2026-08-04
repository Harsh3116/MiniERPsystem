using Microsoft.EntityFrameworkCore;
using MiniERPsystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniERPsystem.Services
{
    // ── Data models returned to the view ───────────────────────────────

    public class InsightCard
    {
        public string Severity  { get; set; } = "info";  // success | info | warning | danger
        public string Icon      { get; set; } = "bi-lightbulb";
        public string Title     { get; set; } = "";
        public string Body      { get; set; } = "";
        public string? ActionLabel { get; set; }
        public string? ActionUrl   { get; set; }
    }

    public class TopSeller
    {
        public string  ProductName { get; set; } = "";
        public int     UnitsSold   { get; set; }
        public decimal Revenue     { get; set; }
        public int     BarWidth    { get; set; }   // 0–100 (percent of max)
    }

    public class TopCustomer
    {
        public string  CustomerName { get; set; } = "";
        public int     OrderCount   { get; set; }
        public decimal TotalSpent   { get; set; }
        public string  Initial      => CustomerName.Length > 0 ? CustomerName[..1].ToUpper() : "?";
    }

    public class StockAlert
    {
        public string ProductName    { get; set; } = "";
        public int    CurrentStock   { get; set; }
        public int    SoldLast30Days { get; set; }
        public int    DaysRemaining  { get; set; }
        public string Urgency        { get; set; } = "low";  // critical | high | medium | low
    }

    public class InsightsDashboard
    {
        public List<InsightCard>  Cards          { get; set; } = new();
        public List<TopSeller>    TopSellers     { get; set; } = new();
        public List<TopCustomer>  TopCustomers   { get; set; } = new();
        public List<StockAlert>   StockAlerts    { get; set; } = new();

        public decimal CurrentMonthRevenue { get; set; }
        public decimal LastMonthRevenue    { get; set; }
        public decimal RevenueChangePct    { get; set; }
        public string  RevenueTrend        { get; set; } = "neutral";

        public int     TotalSalesThisMonth { get; set; }
        public int     TotalSalesLastMonth { get; set; }
    }

    // ── Service ────────────────────────────────────────────────────────

    public class AiInsightsService
    {
        private readonly ApplicationDbContext _db;

        public AiInsightsService(ApplicationDbContext db) => _db = db;

        public InsightsDashboard Generate()
        {
            var today       = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var last30Start    = today.AddDays(-30);

            var dashboard = new InsightsDashboard();

            // ── Revenue month-over-month ──────────────────
            dashboard.CurrentMonthRevenue  = _db.Sales
                .Where(s => s.SaleDate >= thisMonthStart)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            dashboard.LastMonthRevenue = _db.Sales
                .Where(s => s.SaleDate >= lastMonthStart && s.SaleDate < thisMonthStart)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            dashboard.TotalSalesThisMonth = _db.Sales.Count(s => s.SaleDate >= thisMonthStart);
            dashboard.TotalSalesLastMonth = _db.Sales.Count(s => s.SaleDate >= lastMonthStart && s.SaleDate < thisMonthStart);

            if (dashboard.LastMonthRevenue > 0)
            {
                dashboard.RevenueChangePct = Math.Round(
                    (dashboard.CurrentMonthRevenue - dashboard.LastMonthRevenue) / dashboard.LastMonthRevenue * 100, 1);
            }
            dashboard.RevenueTrend = dashboard.RevenueChangePct > 0 ? "up"
                                   : dashboard.RevenueChangePct < 0 ? "down"
                                   : "neutral";

            // ── Top sellers (by units in last 30 days) ────
            var salesItems = _db.SaleItems
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .Where(si => si.Sale != null && si.Sale.SaleDate >= last30Start)
                .ToList();

            var topSellerData = salesItems
                .GroupBy(si => si.Product?.ProductName ?? "Unknown")
                .Select(g => new {
                    Name    = g.Key,
                    Units   = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Price * x.Quantity)
                })
                .OrderByDescending(x => x.Units)
                .Take(5)
                .ToList();

            int maxUnits = topSellerData.FirstOrDefault()?.Units ?? 1;
            dashboard.TopSellers = topSellerData.Select(x => new TopSeller
            {
                ProductName = x.Name,
                UnitsSold   = x.Units,
                Revenue     = x.Revenue,
                BarWidth    = maxUnits > 0 ? (int)Math.Round(x.Units * 100.0 / maxUnits) : 0
            }).ToList();

            // ── Top customers ─────────────────────────────
            dashboard.TopCustomers = _db.Sales
                .GroupBy(s => s.CustomerName)
                .Select(g => new TopCustomer
                {
                    CustomerName = g.Key,
                    OrderCount   = g.Count(),
                    TotalSpent   = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToList();

            // ── Stock alerts (products with recent sales) ─
            var productSalesLast30 = salesItems
                .GroupBy(si => si.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var activeProducts = _db.Products.Where(p => p.IsActive).ToList();

            foreach (var product in activeProducts)
            {
                int soldLast30 = productSalesLast30.TryGetValue(product.Id, out int sold) ? sold : 0;
                if (soldLast30 == 0 && product.StockQuantity > 10) continue;  // ignore unsold items with ample stock

                double dailyRate = soldLast30 / 30.0;
                int daysLeft = dailyRate > 0
                    ? (int)Math.Floor(product.StockQuantity / dailyRate)
                    : 999;

                string urgency = daysLeft <= 3  ? "critical"
                               : daysLeft <= 7  ? "high"
                               : daysLeft <= 14 ? "medium"
                               : product.StockQuantity <= 5 ? "high"
                               : "low";

                if (urgency is "critical" or "high" or "medium")
                {
                    dashboard.StockAlerts.Add(new StockAlert
                    {
                        ProductName    = product.ProductName,
                        CurrentStock   = product.StockQuantity,
                        SoldLast30Days = soldLast30,
                        DaysRemaining  = daysLeft == 999 ? -1 : daysLeft,
                        Urgency        = urgency
                    });
                }
            }
            dashboard.StockAlerts = dashboard.StockAlerts.OrderBy(a => a.DaysRemaining == -1 ? 999 : a.DaysRemaining).ToList();

            // ── Smart AI insight cards ────────────────────
            GenerateCards(dashboard);

            return dashboard;
        }

        private static void GenerateCards(InsightsDashboard d)
        {
            // Revenue trend
            if (d.RevenueTrend == "up")
                d.Cards.Add(new InsightCard
                {
                    Severity = "success",
                    Icon     = "bi-graph-up-arrow",
                    Title    = $"Revenue up {d.RevenueChangePct}% this month",
                    Body     = $"This month: ₹{d.CurrentMonthRevenue:N0} vs last month: ₹{d.LastMonthRevenue:N0}. Keep up the momentum!",
                });
            else if (d.RevenueTrend == "down")
                d.Cards.Add(new InsightCard
                {
                    Severity     = "warning",
                    Icon         = "bi-graph-down-arrow",
                    Title        = $"Revenue down {Math.Abs(d.RevenueChangePct)}% vs last month",
                    Body         = $"This month: ₹{d.CurrentMonthRevenue:N0} vs last month: ₹{d.LastMonthRevenue:N0}. Consider promotions.",
                    ActionLabel  = "View Sales",
                    ActionUrl    = "/Sales/Index"
                });
            else
                d.Cards.Add(new InsightCard
                {
                    Severity = "info",
                    Icon     = "bi-bar-chart-line",
                    Title    = "No prior month data yet",
                    Body     = "Revenue trend will appear once you have data across two months.",
                });

            // Best seller
            if (d.TopSellers.Count > 0)
            {
                var best = d.TopSellers[0];
                d.Cards.Add(new InsightCard
                {
                    Severity = "info",
                    Icon     = "bi-trophy",
                    Title    = $"Top seller: {best.ProductName}",
                    Body     = $"{best.UnitsSold} units sold in the last 30 days, generating ₹{best.Revenue:N0} in revenue.",
                });
            }

            // Stock-out warnings
            var critical = d.StockAlerts.Where(a => a.Urgency == "critical").ToList();
            var high     = d.StockAlerts.Where(a => a.Urgency == "high").ToList();

            if (critical.Count > 0)
            {
                foreach (var a in critical)
                    d.Cards.Add(new InsightCard
                    {
                        Severity    = "danger",
                        Icon        = "bi-exclamation-octagon-fill",
                        Title       = $"URGENT: {a.ProductName} almost out of stock",
                        Body        = $"Only {a.CurrentStock} units left. At current sales rate, stock runs out in ~{a.DaysRemaining} day(s). Reorder immediately.",
                        ActionLabel = "View Products",
                        ActionUrl   = "/Products/Index"
                    });
            }

            if (high.Count > 0 && critical.Count == 0)
            {
                d.Cards.Add(new InsightCard
                {
                    Severity    = "warning",
                    Icon        = "bi-box-seam",
                    Title       = $"{high.Count} product(s) need restocking soon",
                    Body        = string.Join(", ", high.Select(a => $"{a.ProductName} ({a.CurrentStock} left)")) + ". Based on sales velocity, these will run out within a week.",
                    ActionLabel = "View Inventory",
                    ActionUrl   = "/Products/Index"
                });
            }

            // Loyal customer
            if (d.TopCustomers.Count > 0)
            {
                var top = d.TopCustomers[0];
                d.Cards.Add(new InsightCard
                {
                    Severity = "success",
                    Icon     = "bi-person-heart",
                    Title    = $"Most valuable customer: {top.CustomerName}",
                    Body     = $"{top.CustomerName} has placed {top.OrderCount} order(s) totalling ₹{top.TotalSpent:N0}. Consider a loyalty offer.",
                });
            }

            // Sales volume comparison
            if (d.TotalSalesLastMonth > 0)
            {
                int diff = d.TotalSalesThisMonth - d.TotalSalesLastMonth;
                if (diff < 0)
                    d.Cards.Add(new InsightCard
                    {
                        Severity = "info",
                        Icon     = "bi-receipt",
                        Title    = $"Sales volume is {Math.Abs(diff)} order(s) below last month",
                        Body     = $"You have {d.TotalSalesThisMonth} orders this month vs {d.TotalSalesLastMonth} last month. Try following up with inactive customers.",
                    });
            }

            // Healthy stock message
            if (d.StockAlerts.Count == 0)
                d.Cards.Add(new InsightCard
                {
                    Severity = "success",
                    Icon     = "bi-shield-check",
                    Title    = "Inventory is well stocked",
                    Body     = "No products are at risk of running out based on current sales velocity.",
                });
        }
    }
}
