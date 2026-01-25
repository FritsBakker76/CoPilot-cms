
using System;
using Microsoft.EntityFrameworkCore;
using CmsModern.Models;

namespace CmsModern.Data
{
    public class CmsDbContext : DbContext
    {
        public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options) { }

        public DbSet<Page> Pages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PageContent> PageContents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Page>().HasData(
                new Page
                {
                    Id = 1,
                    Title = "Welcome",
                    Description = "Welcome to our website",
                    Content = "Welcome content here.",
                    GoogleTitle = "Welcome Page",
                    GoogleDescription = "Welcome to our site",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Page
                {
                    Id = 2,
                    Title = "Contact",
                    Description = "Contact us",
                    Content = "Contact information here.",
                    GoogleTitle = "Contact Us",
                    GoogleDescription = "Get in touch",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Page
                {
                    Id = 3,
                    Title = "News",
                    Description = "Latest news",
                    Content = "News content here.",
                    GoogleTitle = "News",
                    GoogleDescription = "Stay updated",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            );
        }
    }
}
