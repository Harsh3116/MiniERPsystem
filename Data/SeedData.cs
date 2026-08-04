using MiniERPsystem.Models;
using MiniERPsystem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniERPsystem.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // ── USERS ───────────────────────────────────────
            var userSeeds = new[]
            {
                new { Email = "admin@erp.com",   FullName = "Arjun Mehta",    Password = "admin123",  Role = "Admin" },
                new { Email = "staff1@erp.com",  FullName = "Priya Sharma",   Password = "staff123",  Role = "Staff" },
                new { Email = "staff2@erp.com",  FullName = "Rohan Kapoor",   Password = "staff123",  Role = "Staff" },
            };
            foreach (var u in userSeeds)
            {
                if (!context.Users.Any(x => x.Email == u.Email))
                    context.Users.Add(new User { Email = u.Email, FullName = u.FullName, Password = PasswordHelper.Hash(u.Password), Role = u.Role });
            }
            context.SaveChanges();

            // ── PRODUCTS ────────────────────────────────────
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { ProductName = "Laptop Pro 15\"",      Price = 72000,  StockQuantity = 8,   IsActive = true },
                    new Product { ProductName = "Desktop PC",            Price = 38000,  StockQuantity = 5,   IsActive = true },
                    new Product { ProductName = "Wireless Mouse",        Price = 850,    StockQuantity = 60,  IsActive = true },
                    new Product { ProductName = "Mechanical Keyboard",   Price = 2800,   StockQuantity = 22,  IsActive = true },
                    new Product { ProductName = "USB-C Hub 7-in-1",      Price = 1600,   StockQuantity = 35,  IsActive = true },
                    new Product { ProductName = "Monitor 24\"",          Price = 19500,  StockQuantity = 12,  IsActive = true },
                    new Product { ProductName = "HD Webcam",             Price = 3200,   StockQuantity = 18,  IsActive = true },
                    new Product { ProductName = "Noise-Cancel Headset",  Price = 2200,   StockQuantity = 25,  IsActive = true },
                    new Product { ProductName = "Laser Printer",         Price = 8500,   StockQuantity = 6,   IsActive = true },
                    new Product { ProductName = "Toner Cartridge",       Price = 1250,   StockQuantity = 20,  IsActive = true },
                    new Product { ProductName = "USB Flash Drive 64GB",  Price = 650,    StockQuantity = 50,  IsActive = true },
                    new Product { ProductName = "Ethernet Cable 5m",     Price = 350,    StockQuantity = 80,  IsActive = true }
                );
                context.SaveChanges();
            }

            // ── CUSTOMERS ───────────────────────────────────
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer { CustomerName = "Rahul Sharma",    Phone = "9876543210", Email = "rahul@gmail.com"   },
                    new Customer { CustomerName = "Anita Verma",     Phone = "9123456789", Email = "anita@gmail.com"   },
                    new Customer { CustomerName = "Kiran Patel",     Phone = "9988776655", Email = "kiran@outlook.com" },
                    new Customer { CustomerName = "Suresh Nair",     Phone = "9011223344", Email = "suresh@yahoo.com"  },
                    new Customer { CustomerName = "Meera Joshi",     Phone = "9867452310", Email = "meera@gmail.com"   },
                    new Customer { CustomerName = "Aakash Singh",    Phone = "9741852963", Email = "aakash@gmail.com"  },
                    new Customer { CustomerName = "Pooja Desai",     Phone = "9632587410", Email = "pooja@hotmail.com" },
                    new Customer { CustomerName = "Vijay Reddy",     Phone = "9514723685", Email = "vijay@gmail.com"   },
                    new Customer { CustomerName = "Neha Gupta",      Phone = "9321654780", Email = "neha@gmail.com"    },
                    new Customer { CustomerName = "Amit Choudhary",  Phone = "9185274630", Email = "amit@gmail.com"    }
                );
                context.SaveChanges();
            }

            // ── SALES (historical, spread over 90 days) ─────
            if (!context.Sales.Any())
            {
                var products  = context.Products.ToList();
                var customers = context.Customers.ToList();
                var today     = DateTime.Today;

                // Helper to get customer by name
                Customer C(string name) => customers.First(c => c.CustomerName == name);

                var salesData = new List<(string Customer, DateTime Date, List<(string Product, int Qty)> Items)>
                {
                    // 3 months ago
                    (C("Rahul Sharma").CustomerName,    today.AddDays(-85), new(){ ("Wireless Mouse",3),    ("USB Flash Drive 64GB",2) }),
                    (C("Kiran Patel").CustomerName,     today.AddDays(-82), new(){ ("Laptop Pro 15\"",1),   ("Wireless Mouse",1) }),
                    (C("Suresh Nair").CustomerName,     today.AddDays(-79), new(){ ("Mechanical Keyboard",2),("USB-C Hub 7-in-1",1) }),
                    (C("Anita Verma").CustomerName,     today.AddDays(-76), new(){ ("Monitor 24\"",1) }),
                    (C("Aakash Singh").CustomerName,    today.AddDays(-73), new(){ ("HD Webcam",1),         ("Noise-Cancel Headset",1) }),
                    (C("Pooja Desai").CustomerName,     today.AddDays(-70), new(){ ("Wireless Mouse",5),    ("Ethernet Cable 5m",3) }),
                    (C("Vijay Reddy").CustomerName,     today.AddDays(-67), new(){ ("Desktop PC",1),        ("Mechanical Keyboard",1) }),
                    (C("Neha Gupta").CustomerName,      today.AddDays(-64), new(){ ("Toner Cartridge",2),   ("USB Flash Drive 64GB",4) }),
                    (C("Amit Choudhary").CustomerName,  today.AddDays(-61), new(){ ("Laptop Pro 15\"",1) }),
                    (C("Meera Joshi").CustomerName,     today.AddDays(-58), new(){ ("USB-C Hub 7-in-1",2),  ("Wireless Mouse",2) }),

                    // 2 months ago
                    (C("Rahul Sharma").CustomerName,    today.AddDays(-55), new(){ ("Mechanical Keyboard",1),("USB Flash Drive 64GB",3) }),
                    (C("Kiran Patel").CustomerName,     today.AddDays(-52), new(){ ("Noise-Cancel Headset",1),("Ethernet Cable 5m",2) }),
                    (C("Suresh Nair").CustomerName,     today.AddDays(-49), new(){ ("Monitor 24\"",2) }),
                    (C("Aakash Singh").CustomerName,    today.AddDays(-46), new(){ ("Wireless Mouse",4),    ("USB-C Hub 7-in-1",2) }),
                    (C("Pooja Desai").CustomerName,     today.AddDays(-43), new(){ ("HD Webcam",2) }),
                    (C("Vijay Reddy").CustomerName,     today.AddDays(-40), new(){ ("Toner Cartridge",3),   ("Ethernet Cable 5m",5) }),
                    (C("Neha Gupta").CustomerName,      today.AddDays(-37), new(){ ("Laptop Pro 15\"",1),   ("Wireless Mouse",1) }),
                    (C("Anita Verma").CustomerName,     today.AddDays(-34), new(){ ("USB Flash Drive 64GB",6),("USB-C Hub 7-in-1",1) }),
                    (C("Amit Choudhary").CustomerName,  today.AddDays(-31), new(){ ("Desktop PC",1) }),
                    (C("Meera Joshi").CustomerName,     today.AddDays(-28), new(){ ("Mechanical Keyboard",2),("Toner Cartridge",2) }),

                    // Last month
                    (C("Rahul Sharma").CustomerName,    today.AddDays(-25), new(){ ("Monitor 24\"",1),      ("Wireless Mouse",2) }),
                    (C("Kiran Patel").CustomerName,     today.AddDays(-22), new(){ ("Noise-Cancel Headset",2),("USB Flash Drive 64GB",4) }),
                    (C("Suresh Nair").CustomerName,     today.AddDays(-19), new(){ ("Laptop Pro 15\"",1) }),
                    (C("Aakash Singh").CustomerName,    today.AddDays(-16), new(){ ("Ethernet Cable 5m",6), ("USB-C Hub 7-in-1",3) }),
                    (C("Pooja Desai").CustomerName,     today.AddDays(-13), new(){ ("Wireless Mouse",5),    ("Mechanical Keyboard",1) }),
                    (C("Vijay Reddy").CustomerName,     today.AddDays(-10), new(){ ("Toner Cartridge",4),   ("USB Flash Drive 64GB",3) }),

                    // This week
                    (C("Neha Gupta").CustomerName,      today.AddDays(-6),  new(){ ("HD Webcam",1),         ("USB-C Hub 7-in-1",1) }),
                    (C("Amit Choudhary").CustomerName,  today.AddDays(-4),  new(){ ("Laptop Pro 15\"",1),   ("Wireless Mouse",1) }),
                    (C("Anita Verma").CustomerName,     today.AddDays(-2),  new(){ ("Mechanical Keyboard",1),("Ethernet Cable 5m",4) }),
                    (C("Rahul Sharma").CustomerName,    today.AddDays(-1),  new(){ ("Monitor 24\"",1),      ("Noise-Cancel Headset",1) }),
                };

                foreach (var (customerName, date, items) in salesData)
                {
                    var customer = customers.First(c => c.CustomerName == customerName);
                    var sale = new Sale
                    {
                        CustomerId   = customer.Id,
                        CustomerName = customerName,
                        SaleDate     = date,
                        TotalAmount  = 0
                    };
                    context.Sales.Add(sale);
                    context.SaveChanges();

                    decimal total = 0;
                    foreach (var (productName, qty) in items)
                    {
                        var product = products.First(p => p.ProductName == productName);
                        context.SaleItems.Add(new SaleItem
                        {
                            SaleId    = sale.Id,
                            ProductId = product.Id,
                            Quantity  = qty,
                            Price     = product.Price
                        });
                        total += product.Price * qty;
                        // reduce stock
                        product.StockQuantity = Math.Max(0, product.StockQuantity - qty);
                    }

                    sale.TotalAmount = total;
                    context.SaveChanges();
                }
            }
        }
    }
}
