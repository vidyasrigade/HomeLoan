using HomeLoan.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeLoan.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any model configuration here if needed.
        }
    }
}
