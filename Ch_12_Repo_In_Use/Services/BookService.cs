using Abstracts;


namespace Services;
public class BookService:IBookService
{
    // readonly sınıf üyeleri sadece okunabilir sınıf üyeleridir. 
    // readonly sınıf üyelerinin instance'sı yapıcı metot veya tanımlandığı yerde üretilebilir.
    private readonly List<Book> _bookList;

    public int Count => _bookList.Count;

    public BookService()
    {
        _bookList = new List<Book>()
        {
            new Book(){Id = 1, Title = "Deniz Feneri", Price=84.40},
            new Book(){Id = 2, Title = "Sol Ayağım", Price=159.00},
            new Book(){Id = 3, Title = "Simyacı", Price=190.00}
            // Şeker Portakalı - 190.45
        };
    }

    public List<Book>? GetBooks() => _bookList;

    public Book? GetBookById(int id) => 
        _bookList.FirstOrDefault(b => b.Id.Equals(id));

    public void AddBook(Book newBook)
    {
         newBook.Id = _bookList.Max(b => b.Id) + 1;
         _bookList.Add(newBook);
    }

    public Book UpdateBook(int id, Book editBook)
    {
         Book? book = _bookList.FirstOrDefault(b => b.Id.Equals(id));
         if(book is null)
            throw new BookNotFoundException(id);
            
         book.Title = editBook.Title;
         book.Price = editBook.Price;
         return book;
    }

    public void DeleteBook(int id)
    {
        Book? book = _bookList.FirstOrDefault(b => b.Id.Equals(id));
        if(book is not null)
            _bookList.Remove(book);
        else
            throw new BookNotFoundException(id);
    }
}