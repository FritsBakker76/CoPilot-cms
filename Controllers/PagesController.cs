
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CmsModern.Data;
using CmsModern.Models;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;
using System;

namespace CmsModern.Controllers
{
    [Authorize]
    public class PagesController : Controller
    {
        private readonly CmsDbContext _context;

        public PagesController(CmsDbContext context)
        {
            _context = context;
        }

        // GET: Pages
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pages.ToListAsync());
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
