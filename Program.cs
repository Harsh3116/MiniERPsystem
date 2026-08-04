using Microsoft.EntityFrameworkCore;
using MiniERPsystem.Data;
using MiniERPsystem.Models;
using MiniERPsystem.Helpers;
using MiniERPsystem.Services;
using System.Linq;
using System;

namespace MiniERPsystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<AiInsightsService>();

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

                // Create SQLite database from model
                db.Database.EnsureCreated();

                // Seed demo data
                SeedData.Initialize(db);

                // Ensure admin exists
                if (!db.Users.Any())
                {
                    db.Users.Add(new User
                    {
                        FullName = "Admin",
                        Email = "admin@erp.com",
                        Password = PasswordHelper.Hash("admin123"),
                        Role = "Admin"
                    });

                    db.SaveChanges();
                }
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            var wkhtmlPath = @"C:\Program Files\wkhtmltopdf\bin";
            if (Directory.Exists(wkhtmlPath))
            {
                Rotativa.AspNetCore.RotativaConfiguration.Setup(
                    builder.Environment.WebRootPath,
                    wkhtmlPath
                );
            }

            app.Run();
        }
    }
}
