
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class CategoryRepository : RepositoryBase<Category>
{
    public CategoryRepository(RepositoryContext context) : base(context)
    {
    }

    public Category? Get(int id)
    => _context
    .Categories
    .Include(x => x.Books) // eager loading yaklaşımı
    .FirstOrDefault(x => x.Id == id);
    

    public List<Category> GetAll()
    => _context
    .Categories
    .Include(x => x.Books) // eager loading yaklaşımı
    .ToList();
}