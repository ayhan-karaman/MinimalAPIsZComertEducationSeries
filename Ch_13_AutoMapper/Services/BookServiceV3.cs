using System.ComponentModel.DataAnnotations;
using Abstracts;
using AutoMapper;
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
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(bookDto);
        bool isValid = Validator.TryValidateObject(bookDto, context, validationResults, true);

        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
            throw new ValidationException(errors);
        }
        var book = _mapper.Map<Book>(bookDto);
        _bookRepo.Add(book);
        return book;
    }

    public void DeleteBook(int id)
    {
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

    public Book? GetBookById(int id)
    {
        var book = _bookRepo.Get(id);
        if (book == null)
            throw new BookNotFoundException(id);
        return book;
    }

    public List<Book> GetBooks()
    => _bookRepo.GetAll();

    public Book UpdateBook(int id, BookDtoForUpdate bookDto)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(bookDto);
        var isValid = Validator.TryValidateObject(bookDto, context, validationResults, true);
        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
            throw new ValidationException(errors);
        }
        var book = GetBookById(id);
        book = _mapper.Map(bookDto, book);
        _bookRepo.Update(book!);
        return book!;
    }
}