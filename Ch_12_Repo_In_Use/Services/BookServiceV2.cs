using Abstracts;
using Repositories;


namespace Services;

public class BookServiceV2 : IBookService
{
    private readonly RepositoryContext _context;

    public BookServiceV2(RepositoryContext context)
    {
        _context = context;
    }

    public int Count => _context.Books.Count();

    public void AddBook(Book book)
    {
        _context.Add(book);
        _context.SaveChanges();
    }

    public void DeleteBook(int id)
    {
        var book = _context.Books.FirstOrDefault(x => x.Id == id);
        if(book != null)
        {
            _context.Books.Remove(book);
            _context.SaveChanges();
        }
        else
        {
            throw new BookNotFoundException(id);
        }
    }

    public Book? GetBookById(int id)
    => _context.Books.FirstOrDefault(x => x.Id.Equals(id));

    public List<Book> GetBooks()
    => _context.Books.ToList();

    public Book UpdateBook(int id, Book book)
    {
        var item = _context.Books.FirstOrDefault(x => x.Id == id);
        if(item == null)
            throw new BookNotFoundException(id);
        item.Title = book.Title;
        item.Price = book.Price;
        _context.SaveChanges();
        return item;
    }
}
