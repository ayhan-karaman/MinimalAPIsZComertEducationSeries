
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories;
public class BookRepository : RepositoryBase<Book>
{
    public BookRepository(RepositoryContext context) : base(context)
    {
    }

    public Book? Get(int id)
    => _context
    .Books
    .Include(x => x.Category) // eager loading yaklaşımı
    .FirstOrDefault(x => x.Id == id);
    

    public List<Book> GetAll()
    => _context
    .Books
    .Include(x => x.Category) // eager loading yaklaşımı
    .ToList();

   
}
