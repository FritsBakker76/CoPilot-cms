
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
            var pages = await _context.Pages.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
            return View(pages);
        }

        // POST: Pages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,MenuItem,MenuIcon,GoogleTitle,GoogleDescription")] Page page)
        {
            if (!(User.Identity.IsAuthenticated && User.Claims.Any(c => c.Type == "IsAdmin" && c.Value == "True")))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                // Set display order to max + 1
                var maxOrder = await _context.Pages.AnyAsync() ? await _context.Pages.MaxAsync(p => p.DisplayOrder) : 0;
                page.DisplayOrder = maxOrder + 1;
                _context.Add(page);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Nieuwe pagina succesvol toegevoegd.";
            }
            return RedirectToAction(nameof(Index));
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
                    MenuAlignment = "left",
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
            settings.MenuAlignment = model.MenuAlignment;
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,MenuItem,MenuIcon,GoogleTitle,GoogleDescription,BannerPath")] Page page, IFormFile bannerFile)
        {
            if (id != page.Id) return NotFound();
            if (ModelState.IsValid)
            {
                // Get the existing page from the database
                var existingPage = await _context.Pages.FindAsync(id);
                if (existingPage == null) return NotFound();

                // Update properties from the form
                existingPage.Title = page.Title;
                existingPage.Description = page.Description;
                existingPage.MenuItem = page.MenuItem;
                existingPage.MenuIcon = page.MenuIcon;
                existingPage.GoogleTitle = page.GoogleTitle;
                existingPage.GoogleDescription = page.GoogleDescription;

                // Handle banner file upload
                if (bannerFile != null && bannerFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banners");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + bannerFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await bannerFile.CopyToAsync(fileStream);
                    }

                    // Delete old banner if it exists
                    if (existingPage.BannerPath != null)
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, existingPage.BannerPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    existingPage.BannerPath = "/uploads/banners/" + uniqueFileName;
                }

                _context.Update(existingPage);
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
            // Only the 'admin' user account can edit the admin user
            if (user.Username == "admin" && !string.Equals(User.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only admin can edit the admin user.";
                return RedirectToAction(nameof(Users));
            }
            user.Username = username;
            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }
            user.IsAdmin = isAdmin;
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["Message"] = "User updated successfully.";
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
                TempData["Message"] = "User deleted successfully.";
            }
            else if (user != null && user.Username == "admin")
            {
                TempData["Error"] = "The admin user cannot be deleted.";
            }
            return RedirectToAction(nameof(Users));
        }

        // POST: Pages/MoveUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            if (!(User.Identity.IsAuthenticated && User.Claims.Any(c => c.Type == "IsAdmin" && c.Value == "True")))
            {
                return Forbid();
            }

            var currentPage = await _context.Pages.FindAsync(id);
            if (currentPage == null) return NotFound();

            var pages = await _context.Pages.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
            var currentIndex = pages.FindIndex(p => p.Id == id);

            if (currentIndex > 0)
            {
                var previousPage = pages[currentIndex - 1];
                var tempOrder = currentPage.DisplayOrder;
                currentPage.DisplayOrder = previousPage.DisplayOrder;
                previousPage.DisplayOrder = tempOrder;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Pages/MoveDown
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            if (!(User.Identity.IsAuthenticated && User.Claims.Any(c => c.Type == "IsAdmin" && c.Value == "True")))
            {
                return Forbid();
            }

            var currentPage = await _context.Pages.FindAsync(id);
            if (currentPage == null) return NotFound();

            var pages = await _context.Pages.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
            var currentIndex = pages.FindIndex(p => p.Id == id);

            if (currentIndex < pages.Count - 1)
            {
                var nextPage = pages[currentIndex + 1];
                var tempOrder = currentPage.DisplayOrder;
                currentPage.DisplayOrder = nextPage.DisplayOrder;
                nextPage.DisplayOrder = tempOrder;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Pages/DeletePage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePage(int id)
        {
            if (!(User.Identity.IsAuthenticated && User.Claims.Any(c => c.Type == "IsAdmin" && c.Value == "True")))
            {
                return Forbid();
            }

            var page = await _context.Pages.FindAsync(id);
            if (page == null)
            {
                return NotFound();
            }

            // Delete associated PageContent items first
            var contents = await _context.PageContents.Where(c => c.PageId == id).ToListAsync();
            _context.PageContents.RemoveRange(contents);

            // Delete the page
            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Pagina succesvol verwijderd.";
            return RedirectToAction(nameof(Index));
        }
    }
}
