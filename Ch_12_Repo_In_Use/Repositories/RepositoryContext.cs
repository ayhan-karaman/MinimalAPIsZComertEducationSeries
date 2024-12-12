using Microsoft.EntityFrameworkCore;

namespace Repositories;
public class RepositoryContext : DbContext
{
    public RepositoryContext(DbContextOptions<RepositoryContext> options):base(options)
    {
        
    }
    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Book>().HasData(
            new Book(){Id = 1, Title = "Deniz Feneri", Price=84.40},
            new Book(){Id = 2, Title = "Sol Ayağım", Price=159.00},
            new Book(){Id = 3, Title = "Simyacı", Price=190.00}
        );
    }

}