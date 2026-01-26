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

        public IActionResult PageTemplates(int pageId, string position = "bottom")
        {
            ViewBag.PageId = pageId;
            ViewBag.Position = position;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContent(int pageId, string type, string position = "bottom")
        {
            var contentsForPage = _context.PageContents.Where(pc => pc.PageId == pageId);
            int newPosition;

            if (!string.IsNullOrEmpty(position) && position.Equals("top", StringComparison.OrdinalIgnoreCase))
            {
                // Shift existing items down to make room at the top.
                var ordered = contentsForPage.OrderBy(pc => pc.Position).ToList();
                foreach (var item in ordered)
                {
                    item.Position = item.Position + 1;
                }
                newPosition = 1;
            }
            else
            {
                var maxPos = contentsForPage.Max(pc => (int?)pc.Position) ?? 0;
                newPosition = maxPos + 1;
            }

            var newContent = new PageContent
            {
                PageId = pageId,
                Type = type,
                Title = "Nieuwe alinea",
                Content = "Vervang deze tekst...",
                Position = newPosition,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteContentSection(int pageId, int contentId)
        {
            var existing = _context.PageContents.FirstOrDefault(pc => pc.Id == contentId && pc.PageId == pageId);
            if (existing == null)
            {
                return NotFound();
            }

            _context.PageContents.Remove(existing);
            _context.SaveChanges();

            // Re-sequence positions after deletion to keep ordering compact.
            var items = _context.PageContents.Where(pc => pc.PageId == pageId).OrderBy(pc => pc.Position).ToList();
            var pos = 1;
            foreach (var item in items)
            {
                item.Position = pos++;
            }
            _context.SaveChanges();

            return RedirectToAction("Page", new { id = pageId, edit = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoveContentUp(int pageId, int contentId)
        {
            var current = _context.PageContents.FirstOrDefault(pc => pc.Id == contentId && pc.PageId == pageId);
            if (current == null) return NotFound();

            var previous = _context.PageContents.FirstOrDefault(pc => pc.PageId == pageId && pc.Position == current.Position - 1);
            if (previous == null) return RedirectToAction("Page", new { id = pageId, edit = true });

            // Swap positions
            var temp = current.Position;
            current.Position = previous.Position;
            previous.Position = temp;

            _context.SaveChanges();
            return RedirectToAction("Page", new { id = pageId, edit = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoveContentDown(int pageId, int contentId)
        {
            var current = _context.PageContents.FirstOrDefault(pc => pc.Id == contentId && pc.PageId == pageId);
            if (current == null) return NotFound();

            var next = _context.PageContents.FirstOrDefault(pc => pc.PageId == pageId && pc.Position == current.Position + 1);
            if (next == null) return RedirectToAction("Page", new { id = pageId, edit = true });

            // Swap positions
            var temp = current.Position;
            current.Position = next.Position;
            next.Position = temp;

            _context.SaveChanges();
            return RedirectToAction("Page", new { id = pageId, edit = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePageHeader(int pageId, string pageTitle, string pageDescription)
        {
            var page = _context.Pages.Find(pageId);
            if (page == null)
            {
                return NotFound();
            }

            page.Title = pageTitle;
            page.Description = pageDescription;

            _context.SaveChanges();
            return RedirectToAction("Page", new { id = pageId, edit = true });
        }
    }
}