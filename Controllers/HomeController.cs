using Microsoft.AspNetCore.Mvc;
using CmsModern.Data;
using System.Linq;
using System.Collections.Generic;
using CmsModern.Models;
using System;

namespace CmsModern.Controllers
{
    public class HomeController : Controller
    {
        private readonly CmsDbContext _context;

        public HomeController(CmsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Page", new { id = 1 });
        }

        public IActionResult Page(int id, bool edit = false)
        {
            var page = _context.Pages.Find(id);
            if (page == null)
            {
                return NotFound();
            }
            var contents = _context.PageContents.Where(pc => pc.PageId == id).OrderBy(pc => pc.Position).ToList();
            ViewBag.Contents = contents;
            ViewBag.EditMode = edit && User.Identity.IsAuthenticated;
            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveContents(int pageId, string pageTitle, string pageDescription, List<PageContent> Contents)
        {
            var page = _context.Pages.Find(pageId);
            if (page != null)
            {
                page.Title = pageTitle;
                page.Description = pageDescription;
            }
            foreach (var c in Contents)
            {
                var existing = _context.PageContents.Find(c.Id);
                if (existing != null)
                {
                    existing.Title = c.Title;
                    existing.Content = c.Content;
                    existing.Link = c.Link;
                    existing.Price = c.Price;
                    existing.Duration = c.Duration;
                    existing.PictureText = c.PictureText;
                    existing.Type = c.Type;
                    existing.Position = c.Position;
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Page", new { id = pageId });
        }

        public IActionResult PageTemplates(int pageId)
        {
            ViewBag.PageId = pageId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContent(int pageId, string type)
        {
            var maxPos = _context.PageContents.Where(pc => pc.PageId == pageId).Max(pc => (int?)pc.Position) ?? 0;
            var newContent = new PageContent
            {
                PageId = pageId,
                Type = type,
                Position = maxPos + 1,
                Created = DateTime.Now
            };
            _context.PageContents.Add(newContent);
            _context.SaveChanges();
            return RedirectToAction("Page", new { id = pageId, edit = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveContentSection(int pageId, PageContent model)
        {
            var existing = _context.PageContents.FirstOrDefault(pc => pc.Id == model.Id && pc.PageId == pageId);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Title = model.Title;
            existing.Content = model.Content;
            existing.PictureText = model.PictureText;
            existing.Type = model.Type;
            existing.Position = model.Position;
            existing.Link = model.Link;
            existing.Price = model.Price;
            existing.Duration = model.Duration;

            _context.SaveChanges();
            return RedirectToAction("Page", new { id = pageId, edit = true });
        }
    }
}