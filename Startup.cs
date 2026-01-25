
using CmsModern.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql;
using BCrypt.Net;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CmsModern
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                });
            services.AddDbContext<CmsDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"))));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "login",
                    pattern: "login",
                    defaults: new { controller = "Account", action = "Login" });
                endpoints.MapControllerRoute(
                    name: "account",
                    pattern: "Account/{action}",
                    defaults: new { controller = "Account" });
                endpoints.MapControllerRoute(
                    name: "admin",
                    pattern: "admin/{action=Index}/{id?}",
                    defaults: new { controller = "Pages" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Page}/{id=1}");
            });

            // Seed the database
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
                context.Database.EnsureCreated();
                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS pagecontent (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    title VARCHAR(255),
                    content TEXT,
                    link VARCHAR(500),
                    price DECIMAL(18,2),
                    duration VARCHAR(100),
                    pictureText VARCHAR(500),
                    type VARCHAR(100),
                    pageId INT,
                    position INT,
                    created DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (pageId) REFERENCES pages(id)
                );");
                // Insert sample data if not exists
                if (!context.PageContents.Any())
                {
                    context.Database.ExecuteSqlRaw(@"INSERT INTO pagecontent (title, content, link, price, duration, pictureText, type, pageId, position) VALUES
                        ('Hero Section', 'Welcome to our amazing website!', 'https://example.com', 0.00, 'N/A', 'Hero image', 'hero', 1, 1),
                        ('About Us', 'We are a great company.', NULL, NULL, NULL, NULL, 'text', 1, 2);");
                }
                if (!context.Pages.Any())
                {
                    context.Pages.AddRange(
                        new Models.Page
                        {
                            Title = "Welcome",
                            Description = "Welcome to our website",
                            Content = "Welcome content here.",
                            GoogleTitle = "Welcome Page",
                            GoogleDescription = "Welcome to our site",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Models.Page
                        {
                            Title = "Contact",
                            Description = "Contact us",
                            Content = "Contact information here.",
                            GoogleTitle = "Contact Us",
                            GoogleDescription = "Get in touch",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Models.Page
                        {
                            Title = "News",
                            Description = "Latest news",
                            Content = "News content here.",
                            GoogleTitle = "News",
                            GoogleDescription = "Stay updated",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    );
                    context.SaveChanges();
                }
                if (!context.Users.Any(u => u.Username == "admin"))
                {
                    context.Users.Add(new Models.User
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                        IsAdmin = true
                    });
                    context.SaveChanges();
                }
                else
                {
                    var admin = context.Users.First(u => u.Username == "admin");
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin");
                    admin.IsAdmin = true;
                    context.SaveChanges();
                }
            }
        }
    }
}
