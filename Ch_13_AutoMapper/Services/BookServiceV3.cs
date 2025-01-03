using System.ComponentModel.DataAnnotations;
using Abstracts;
using AutoMapper;
using Configuration;
using Entities;
using Entities.DTOs;
using Entities.Exceptions;
using Repositories;

namespace Services;
public class BookServiceV3 : IBookService
{
    private readonly BookRepository _bookRepo;
    private readonly IMapper _mapper;

    public BookServiceV3(BookRepository bookRepo, IMapper mapper)
    {
        _bookRepo = bookRepo;
        _mapper = mapper;
    }

    public int Count => _bookRepo.GetAll().Count;

    public Book AddBook(BookDtoForInsertion bookDto)
    {
        bookDto.Validate();
        var book = _mapper.Map<Book>(bookDto);
        _bookRepo.Add(book);
        return book;
    }

    public void DeleteBook(int id)
    {
        id.ValidationIdInRange();
        var book = _bookRepo.Get(id);
        if (book != null)
        {
            _bookRepo.Remove(book);
        }
        else
        {
            throw new BookNotFoundException(id);
        }
    }

    public BookDto? GetBookById(int id)
    {
        id.ValidationIdInRange();
        var book = _bookRepo.Get(id);
        if (book == null)
            throw new BookNotFoundException(id);
        return _mapper.Map<BookDto>(book);
    }

    public List<BookDto> GetBooks()
    {
         var books = _bookRepo.GetAll();
         return _mapper.Map<List<BookDto>>(books);
    }

    public Book UpdateBook(int id, BookDtoForUpdate bookDto)
    {
        id.ValidationIdInRange();
        bookDto.Validate();
        var book = _bookRepo.Get(id);
        if(book == null)
            throw new BookNotFoundException(id);
        
        book = _mapper.Map(bookDto, book);
        _bookRepo.Update(book!);
        return book!;
    }

    private void Validate<T>(T item)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(item);
        var isValid = Validator.TryValidateObject(item, context, validationResults, true);
        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
            throw new ValidationException(errors);
        }
    }
}