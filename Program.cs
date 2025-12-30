using Microsoft.EntityFrameworkCore;
using MiniERPsystem.Data;
using MiniERPsystem.Models;
using System.Linq;   // ✅ REQUIRED
using System;        // ✅ REQUIRED

namespace MiniERPsystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllersWithViews();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            

            var app = builder.Build();

            // ✅ DATABASE SEEDING (SAFE)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Seed demo data
                SeedData.Initialize(db);

                // Ensure admin exists
                if (!db.Users.Any())
                {
                    db.Users.Add(new User
                    {
                        FullName = "Admin",
                        Email = "admin@erp.com",
                        Password = "admin123",
                        Role = "Admin"
                    });

                    db.SaveChanges();
                }
            }
            builder.Services.AddSession();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");
            Rotativa.AspNetCore.RotativaConfiguration.Setup(
    builder.Environment.WebRootPath,
    @"C:\Program Files\wkhtmltopdf\bin"
);
            app.Run();
        }
    }
}
