
using Microsoft.EntityFrameworkCore;
using CmsModern.Models;

namespace CmsModern.Data
{
    public class CmsDbContext : DbContext
    {
        public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options) { }

        public DbSet<Page> Pages { get; set; }
        // public DbSet<User> Users { get; set; } // Voor uitbreiding met gebruikersbeheer
    }
}
