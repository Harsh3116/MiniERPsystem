using MiniERPsystem.Models;
using System;
using System.Linq;

namespace MiniERPsystem.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // USERS
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        FullName = "Admin",
                        Email = "admin@erp.com",
                        Password = "admin123",
                        Role = "Admin"
                    },
                    new User
                    {
                        FullName = "Staff User",
                        Email = "staff@erp.com",
                        Password = "staff123",
                        Role = "Staff"
                    }
                );
            }

            // PRODUCTS
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { ProductName = "Laptop", Price = 50000, StockQuantity = 10 },
                    new Product { ProductName = "Mouse", Price = 500, StockQuantity = 50 },
                    new Product { ProductName = "Keyboard", Price = 1500, StockQuantity = 30 }
                );
            }

            // CUSTOMERS
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer { CustomerName = "Rahul Sharma", Phone = "9876543210", Email = "rahul@gmail.com" },
                    new Customer { CustomerName = "Anita Verma", Phone = "9123456789", Email = "anita@gmail.com" }
                );
            }

            context.SaveChanges();
        }
    }
}
