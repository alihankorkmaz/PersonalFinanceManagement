using Microsoft.EntityFrameworkCore;
using PersonalFinanceManagement.Models;

public class FinanceContext : DbContext
{
    public FinanceContext(DbContextOptions<FinanceContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<AdminSettings> AdminSettings { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)"); // precision: 18, scale: 2
    }
}
