using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Repositories;
public class RepositoryContext : IdentityDbContext<User>
{
    public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
    {

    }
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                CategoryName = "Felsefe"
            },
            new Category
            {
                Id = 2,
                CategoryName = "Hikaye"
            },
            new Category
            {
                Id = 3,
                CategoryName = "Deneme"
            }
        );
        modelBuilder.Entity<Book>().HasData(
            new Book()
            {
                Id = 1,
                URL = "/images/1.jpg",
                Title = "Devlet",
                CategoryId = 1,
                Price = 84.40
            },
            new Book()
            {
                Id = 2,
                URL = "/images/2.jpg",
                Title = "Ateşten Gömlek",
                CategoryId = 2,
                Price = 159.00
            },
            new Book()
            {
                Id = 3,
                URL = "/images/3.jpg",
                Title = "Huzur",
                CategoryId = 3,
                Price = 190.00
            }
        );
        

        modelBuilder.Entity<IdentityRole>().HasData(
             new IdentityRole()
             {
                 Name = "Admin",
                 NormalizedName =  "ADMIN"
             },
             new  IdentityRole()
             {
                Name = "User",
                NormalizedName = "USER"
             }
        );
    
    }

}