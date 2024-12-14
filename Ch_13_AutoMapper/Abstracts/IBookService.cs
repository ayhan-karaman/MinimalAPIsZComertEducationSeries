
using Entities;
using Entities.DTOs;

namespace Abstracts;
public interface IBookService
{
     int Count { get; }
     List<Book> GetBooks();
     Book? GetBookById(int id);
     Book AddBook(BookDtoForInsertion bookDto);
     Book UpdateBook(int id, BookDtoForUpdate bookDto);
     void DeleteBook(int id);
}
