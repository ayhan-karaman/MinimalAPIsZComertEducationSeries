using Abstracts;
using Repositories;

namespace Services;
public class BookServiceV3 : IBookService
{
    private readonly BookRepository _bookRepo;

    public BookServiceV3(BookRepository bookRepo)
    {
        _bookRepo = bookRepo;
    }

    public int Count => _bookRepo.GetAll().Count;

    public void AddBook(Book book)
    {
        _bookRepo.Add(book);
    }

    public void DeleteBook(int id)
    {
        var book = _bookRepo.Get(id);
        if(book != null)
        {
            _bookRepo.Remove(book);
        }
        else
        {
            throw new BookNotFoundException(id);
        }
    }

    public Book? GetBookById(int id)
    => _bookRepo.Get(id);

    public List<Book> GetBooks()
    => _bookRepo.GetAll();

    public Book UpdateBook(int id, Book book)
    {
        var item =_bookRepo.Get(id);
        if(item == null)
            throw new BookNotFoundException(id);
        item.Title = book.Title;
        item.Price = book.Price;
        _bookRepo.Update(item);
        return item;
    }
}