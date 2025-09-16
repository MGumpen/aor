using Microsoft.EntityFrameworkCore;

namespace AOR.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    // DbSets for your entities will go here
    // Example: public DbSet<User> Users { get; set; }
    public DbSet<AdviceDto> Advices { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdviceDto>().HasKey(keyId => keyId.AdviceId);
        {
            
        };
    }

  //  protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //  base.OnModelCreating(modelBuilder);

    // Entity configurations will go here
    // Example:
    // modelBuilder.Entity<User>(entity =>
    // {
    //     entity.HasKey(e => e.Id);
    //     entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
    // });
    // }
}