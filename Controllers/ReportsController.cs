using MiniERPsystem.Data;
using MiniERPsystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using ClosedXML.Excel;
using System.IO;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;


namespace MiniERPsystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult DailySales()
        {
            var dailySales = _context.Sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DailySalesReport
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

          
            ViewBag.Labels = dailySales.Select(d => d.Date.ToShortDateString()).ToList();
            ViewBag.Amounts = dailySales.Select(d => d.TotalAmount).ToList();

            return View(dailySales);
        }


        public IActionResult MonthlySales()
        {
            var monthlySales = _context.Sales
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                .Select(g => new MonthlySalesReport
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            ViewBag.Labels = monthlySales
                .Select(m => $"{m.Month}/{m.Year}")
                .ToList();

            ViewBag.Amounts = monthlySales
                .Select(m => m.TotalAmount)
                .ToList();

            return View(monthlySales);
        }
        public IActionResult DateRangeSales(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Sales.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.SaleDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(s => s.SaleDate.Date <= toDate.Value.Date);

            var data = query
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DailySalesReport
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Chart data
            ViewBag.Labels = data.Select(d => d.Date.ToShortDateString()).ToList();
            ViewBag.Amounts = data.Select(d => d.TotalAmount).ToList();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(data);
        }


        public IActionResult ExportDateRangeSales(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Sales.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.SaleDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(s => s.SaleDate.Date <= toDate.Value.Date);

            var data = query
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sales Report");

            // Header
            worksheet.Cell(1, 1).Value = "Date";
            worksheet.Cell(1, 2).Value = "Total Sales";

            // Data
            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.Date.ToShortDateString();
                worksheet.Cell(row, 2).Value = item.TotalAmount;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "SalesReport.xlsx"
            );
        }

        public IActionResult ExportDateRangeSalesPdf(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Sales.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.SaleDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(s => s.SaleDate.Date <= toDate.Value.Date);

            var data = query
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DailySalesReport
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.FromDate = fromDate?.ToString("dd-MM-yyyy");
            ViewBag.ToDate = toDate?.ToString("dd-MM-yyyy");

            return new ViewAsPdf("DateRangeSalesPdf", data)
            {
                FileName = "SalesReport.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait
            };
        }


    }

}
