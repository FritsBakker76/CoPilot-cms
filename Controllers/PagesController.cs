
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CmsModern.Data;
using CmsModern.Models;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace CmsModern.Controllers
{
    [Authorize]
    public class PagesController : Controller
    {
        private readonly CmsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PagesController(CmsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Pages
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pages.ToListAsync());
        }

        // GET: Admin Settings
        public async Task<IActionResult> Settings()
        {
            if (!(User.Identity.IsAuthenticated && User.Claims.Any(c => c.Type == "IsAdmin" && c.Value == "True")))
            {
                return Forbid();
            }
            var settings = await _context.WebsiteSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new WebsiteSettings
                {
                    SiteTitle = "CMS MODERN",
                    HeaderBg = "#f8f9fa",
                    SiteBg = "#ffffff",
                    FooterBg = "#f8f9fa",
                    FontPageTitle = 28,
                    FontAlineaTitle = 22,
                    FontWebsiteText = 16,
                    FontSlideshowFooter = 14
                };
                _context.WebsiteSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return View(settings);
        }

        // POST: Admin Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(WebsiteSettings model, IFormFile logo)
        {
            if (!(User.Identity.IsAuthenticated && User.Claims.Any(c => c.Type == "IsAdmin" && c.Value == "True")))
            {
                return Forbid();
            }
            var settings = await _context.WebsiteSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new WebsiteSettings();
                _context.WebsiteSettings.Add(settings);
            }

            settings.SiteTitle = model.SiteTitle;
            settings.HeaderBg = model.HeaderBg;
            settings.HeaderTextColor = model.HeaderTextColor;
            settings.MenuBg = model.MenuBg;
            settings.MenuTextColor = model.MenuTextColor;
            settings.SiteBg = model.SiteBg;
            settings.SiteTextColor = model.SiteTextColor;
            settings.FooterBg = model.FooterBg;
            settings.FooterTextColor = model.FooterTextColor;
            settings.FontPageTitle = model.FontPageTitle;
            settings.FontAlineaTitle = model.FontAlineaTitle;
            settings.FontWebsiteText = model.FontWebsiteText;
            settings.FontSlideshowFooter = model.FontSlideshowFooter;
            settings.FooterContact = model.FooterContact;
            settings.FooterOpeningHours = model.FooterOpeningHours;
            settings.FooterSocial = model.FooterSocial;

            if (logo != null && logo.Length > 0)
            {
                var ext = Path.GetExtension(logo.FileName).ToLowerInvariant();
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".svg" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("LogoPath", "Only PNG, JPG, JPEG, or SVG allowed.");
                    return View(settings);
                }
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                var fileName = $"logo{ext}";
                var fullPath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }
                settings.LogoPath = $"/uploads/{fileName}";
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Website instellingen bijgewerkt.";
            return RedirectToAction(nameof(Settings));
        }

        // GET: Pages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return NotFound();
            return View(page);
        }

        // POST: Pages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Content,GoogleTitle,GoogleDescription")] Page page)
        {
            if (id != page.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(page);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(page);
        }

        // GET: Users
        public async Task<IActionResult> Users(int? editId)
        {
            ViewBag.EditId = editId;
            return View(await _context.Users.ToListAsync());
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string username, string password, bool isAdmin)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Username and password are required.";
                return RedirectToAction(nameof(Users));
            }
            if (_context.Users.Any(u => u.Username == username))
            {
                TempData["Error"] = "Username already exists.";
                return RedirectToAction(nameof(Users));
            }
            try
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    IsAdmin = isAdmin
                };
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Message"] = "User added successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to add user: {ex.Message}";
            }
            return RedirectToAction(nameof(Users));
        }

        // POST: Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, string username, string password, bool isAdmin)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Username = username;
            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }
            user.IsAdmin = isAdmin;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        // POST: Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && user.Username != "admin")
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
