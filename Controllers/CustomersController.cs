using Microsoft.AspNetCore.Mvc;
using MiniERPsystem.Data;
using MiniERPsystem.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MiniERPsystem.Controllers
{
    public class CustomersController : BaseController
    {
        public CustomersController(ApplicationDbContext context)
    : base(context)
        {
        }
        public IActionResult Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.CustomerName.Contains(search) ||
                                         (c.Phone != null && c.Phone.Contains(search)) ||
                                         (c.Email != null && c.Email.Contains(search)));

            int total = query.Count();

            var customers = query
                .OrderBy(c => c.CustomerName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.TotalCount = total;

            return View(customers);
        }
        

        public IActionResult SalesHistory(int id)
        {
        var customer = _context.Customers
            .Include(c => c.Sales)
            .FirstOrDefault(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
        }
        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            _context.Customers.Add(customer);
            _context.SaveChanges();

            TempData["Success"] = $"Customer \"{customer.CustomerName}\" added successfully.";
            return RedirectToAction(nameof(Index));
        }


    }
}
