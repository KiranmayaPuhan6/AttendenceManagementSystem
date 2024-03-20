using AMS.Entities.Models.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace AMS.Entities.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            try
            {
                var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreator != null)
                {
                    if (!databaseCreator.CanConnect()) databaseCreator.Create();
                    if (!databaseCreator.HasTables()) databaseCreator.CreateTables();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           // optionsBuilder.UseLazyLoadingProxies();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attendence>()
                 .HasOne(a => a.User)
                 .WithMany(u => u.Attendence)
                 .HasForeignKey(a => a.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Leave>()
                .HasOne(a => a.User)
                .WithMany(u => u.Leave)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Attendence> Attendences { get; set; }
        public virtual DbSet<Leave> Leaves { get; set; }
        public virtual DbSet<Holidays> Holidays { get; set; }
        public virtual DbSet<Manager> Managers { get; set; }
    }
}
