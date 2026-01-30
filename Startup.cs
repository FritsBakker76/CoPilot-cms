
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
using System.Data.Common;

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
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 21))));
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
                // HTTPS redirection and HSTS disabled for now; enable in production when ready
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
                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS pages (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    title VARCHAR(255),
                    description TEXT,
                    menu_item VARCHAR(255),
                    google_title VARCHAR(255),
                    google_description TEXT,
                    banner_path VARCHAR(500),
                    created_at DATETIME,
                    updated_at DATETIME
                );");
                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS users (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    username VARCHAR(50) NOT NULL UNIQUE,
                    password_hash VARCHAR(255) NOT NULL,
                    is_admin BOOLEAN DEFAULT FALSE
                );");
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
                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS websitesettings (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    logoPath VARCHAR(500),
                    siteTitle VARCHAR(200),
                    headerBg VARCHAR(20),
                    headerTextColor VARCHAR(20),
                    siteBg VARCHAR(20),
                    siteTextColor VARCHAR(20),
                    footerBg VARCHAR(20),
                    footerTextColor VARCHAR(20),
                    fontPageTitle INT,
                    fontAlineaTitle INT,
                    fontWebsiteText INT,
                    fontSlideshowFooter INT,
                    footerContact TEXT,
                    footerOpeningHours TEXT,
                    footerSocial TEXT
                );");
                
                // Add banner_path column to pages table if it doesn't exist
                try
                {
                    var provider = context.Database.ProviderName ?? string.Empty;
                    var conn = context.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    using var cmd = conn.CreateCommand();
                    if (provider.IndexOf("mysql", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pages' AND COLUMN_NAME = 'banner_path';";
                    }
                    else
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('pages') WHERE name = 'banner_path';";
                    }

                    var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    if (!exists)
                    {
                        context.Database.ExecuteSqlRaw("ALTER TABLE pages ADD COLUMN banner_path VARCHAR(500)");
                    }
                }
                catch
                {
                    // Ignore if the column already exists or the check fails; startup should continue.
                }

                // Add menu_item column to pages table if it doesn't exist
                try
                {
                    var provider = context.Database.ProviderName ?? string.Empty;
                    var conn = context.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    using var cmd = conn.CreateCommand();
                    if (provider.IndexOf("mysql", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pages' AND COLUMN_NAME = 'menu_item';";
                    }
                    else
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('pages') WHERE name = 'menu_item';";
                    }

                    var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    if (!exists)
                    {
                        context.Database.ExecuteSqlRaw("ALTER TABLE pages ADD COLUMN menu_item VARCHAR(255)");
                    }
                }
                catch
                {
                    // Ignore if the column already exists or the check fails; startup should continue.
                }
                
                // Add display_order column to pages table if it doesn't exist
                try
                {
                    var provider = context.Database.ProviderName ?? string.Empty;
                    var conn = context.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    using var cmd = conn.CreateCommand();
                    if (provider.IndexOf("mysql", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pages' AND COLUMN_NAME = 'display_order';";
                    }
                    else
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('pages') WHERE name = 'display_order';";
                    }

                    var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    if (!exists)
                    {
                        context.Database.ExecuteSqlRaw("ALTER TABLE pages ADD COLUMN display_order INT NOT NULL DEFAULT 0");
                        // Set display_order to Id for existing pages
                        context.Database.ExecuteSqlRaw("UPDATE pages SET display_order = id WHERE display_order = 0");
                    }
                }
                catch
                {
                    // Ignore if the column already exists or the check fails; startup should continue.
                }
                
                if (!context.Pages.Any())
                {
                    context.Pages.AddRange(
                        new Models.Page
                        {
                            Title = "Welcome",
                            Description = "Welcome to our website",
                            MenuItem = "Welcome",
                            GoogleTitle = "Welcome Page",
                            GoogleDescription = "Welcome to our site",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Models.Page
                        {
                            Title = "Contact",
                            Description = "Contact us",
                            MenuItem = "Contact",
                            GoogleTitle = "Contact Us",
                            GoogleDescription = "Get in touch",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Models.Page
                        {
                            Title = "News",
                            Description = "Latest news",
                            MenuItem = "News",
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

                if (!context.WebsiteSettings.Any())
                {
                    context.WebsiteSettings.Add(new Models.WebsiteSettings
                    {
                        SiteTitle = "CMS MODERN",
                        HeaderBg = "#f8f9fa",
                        HeaderTextColor = "#000000",
                        MenuBg = "#f8f9fa",
                        MenuTextColor = "#000000",
                        SiteBg = "#ffffff",
                        SiteTextColor = "#333333",
                        FooterBg = "#f8f9fa",
                        FooterTextColor = "#000000",
                        FontPageTitle = 28,
                        FontAlineaTitle = 22,
                        FontWebsiteText = 16,
                        FontSlideshowFooter = 14,
                        FooterContact = "",
                        FooterOpeningHours = "",
                        FooterSocial = ""
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
