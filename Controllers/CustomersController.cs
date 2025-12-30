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
        public IActionResult Index()
        {
            var customers = _context.Customers.ToList();
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

            return RedirectToAction(nameof(Index));
        }


    }
}
