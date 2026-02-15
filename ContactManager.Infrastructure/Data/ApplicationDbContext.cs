using ContactManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure auto-increment for SQLite
                if (Database.IsSqlite())
                {
                    entity.Property(e => e.Id).ValueGeneratedOnAdd();
                }
                else
                {
                    // Use identity for SQL Server
                    entity.Property(e => e.Id).UseIdentityColumn();
                }
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            });
        }
    }
}
