using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniERPsystem.Data;
using MiniERPsystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MiniERPsystem.Controllers
{
    public class ProductsController : BaseController
    {

        public ProductsController(ApplicationDbContext context)
    : base(context)
        {
        }

        // GET: Products
        public IActionResult Index(string searchString, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();
            // SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(searchString));
            }

            // TOTAL COUNT (for pagination)
            int totalItems = query.Count();

            // PAGINATION
            var products = query
                .OrderBy(p => p.ProductName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // SEND DATA TO VIEW
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductName,Price,StockQuantity")] Product product)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                LogActivity("Created product: " + product.ProductName);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products
                .FirstOrDefault(p => p.Id == id && p.IsActive);
            
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductName,Price,StockQuantity")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    LogActivity("Edited product: " + product.ProductName);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // SOFT DELETE
            product.IsActive = false;

            _context.SaveChanges();

            LogActivity($"Soft deleted product: {product.ProductName}");

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportToExcel()
        {
            // Admin check
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var products = _context.Products.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");

                // HEADER
                worksheet.Cell(1, 1).Value = "Product Name";
                worksheet.Cell(1, 2).Value = "Price";
                worksheet.Cell(1, 3).Value = "Stock Quantity";

                int row = 2;

                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.ProductName;
                    worksheet.Cell(row, 2).Value = product.Price;
                    worksheet.Cell(row, 3).Value = product.StockQuantity;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ProductsReport.xlsx"
                    );
                }
            } }
            public IActionResult Deleted()
        {
            // Admin-only access
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var deletedProducts = _context.Products
                .Where(p => !p.IsActive)
                .ToList();

            return View(deletedProducts);
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restore(int id)
        {
            // Admin-only access
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = true;
            _context.SaveChanges();

            LogActivity($"Restored product: {product.ProductName}");

            return RedirectToAction(nameof(Deleted));
        }
      
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

    }
}
