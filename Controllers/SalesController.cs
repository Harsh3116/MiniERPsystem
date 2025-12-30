using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using MiniERPsystem.Data;          
using MiniERPsystem.Models;

namespace MiniERPsystem.Controllers
{
    public class SalesController : BaseController


    {
        public SalesController(ApplicationDbContext context)
      : base(context)
        {
        }
        public IActionResult Index()
        {
            var sales = _context.Sales.ToList();
            return View(sales);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sale sale)
        {
            if (!ModelState.IsValid)
            {
                return View(sale);
            }

            var existingCustomer = _context.Customers
                .FirstOrDefault(c => c.CustomerName == sale.CustomerName);

            if (existingCustomer == null)
            {
                existingCustomer = new Customer
                {
                    CustomerName = sale.CustomerName
                };

                _context.Customers.Add(existingCustomer);
                _context.SaveChanges();
            }

            sale.CustomerId = existingCustomer.Id;
            sale.SaleDate = DateTime.Now;
            sale.TotalAmount = 0;

            _context.Sales.Add(sale);
            _context.SaveChanges();

            LogActivity($"Created sale for customer: {existingCustomer.CustomerName}");

            return RedirectToAction("AddItem", new { saleId = sale.Id });
        }

        // GET: Sales/AddItem
        [HttpGet]
        public IActionResult AddItem(int saleId)
        {
            ViewBag.SaleId = saleId;
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // POST: Sales/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(int saleId, int productId, int quantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                return Content("Product not found");
            }

            if (product.StockQuantity < quantity)
            {
                return Content("Not enough stock available");
            }

            var saleItem = new SaleItem
            {
                SaleId = saleId,
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price
            };

            product.StockQuantity -= quantity;

            _context.SaleItems.Add(saleItem);
            _context.SaveChanges();

            // Recalculate total
            decimal total = _context.SaleItems
                .Where(si => si.SaleId == saleId)
                .Select(si => si.Price * si.Quantity)
                .Sum();

            var sale = _context.Sales.Find(saleId);
            if (sale != null)
            {
                sale.TotalAmount = total;
                _context.SaveChanges();
            }

            return RedirectToAction("AddItem", new { saleId = saleId });
        }


        public IActionResult Details(int id)
        {
            var sale = _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        public IActionResult Delete(int id)
        {
            var sale = _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var sale = _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefault(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            // 1. Restore stock
            foreach (var item in sale.SaleItems)
            {
                item.Product.StockQuantity += item.Quantity;
            }

            // 2. Remove sale items
            _context.SaleItems.RemoveRange(sale.SaleItems);

            // 3. Remove sale
            _context.Sales.Remove(sale);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
