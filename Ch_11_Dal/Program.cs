using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Book API",
        Version = "V1",
        Description = "Virtual campus minimal api training program",
        License = new(),
        TermsOfService = new("https://www.samsun.edu.tr"),
        Contact = new()
        {
            Email = "example@example.com.tr",
            Name = "Ayhan Karaman",
            Url = new("https://www.youtube.com/@virtual.campus")
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("all", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });

    options.AddPolicy("special", builder =>
    {
        builder.WithOrigins("https://localhost:300")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// DI registration
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddDbContext<RepositoryContext>(options => {
     options.UseSqlite(builder.Configuration.GetConnectionString("sqlite"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("all");
app.UseHttpsRedirection();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var contextFeatures = context.Features.Get<IExceptionHandlerFeature>();

        if (contextFeatures is not null)
        {
            context.Response.StatusCode = contextFeatures.Error switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status422UnprocessableEntity,
                ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsync(
                    new ErrorDetails()
                    {
                        Message = contextFeatures.Error.Message,
                        StatusCode = context.Response.StatusCode
                    }.ToString()
             );
        }
    });
});

app.MapGet("/api/errors", () =>
{
    throw new Exception("An error has been occured.");
})
.Produces<ErrorDetails>(StatusCodes.Status500InternalServerError)
.ExcludeFromDescription();



app.MapGet("/api/books", (IBookService bookService) =>
{
    return bookService.Count > 0
        ? Results.Ok(bookService.GetBooks())
        : Results.NoContent();
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CRUD", "GETs");



app.MapGet("/api/books/{id:int}", (int id, IBookService bookService) =>
{

    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    var book = bookService.GetBookById(id);
    if (book is not null)
        return Results.Ok(book);
    throw new BookNotFoundException(id); //Results.NotFound();
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("GETs");



app.MapPost("/api/books", (Book newBook, IBookService bookService) =>
{

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(newBook);
    bool isValid = Validator.TryValidateObject(newBook, context, validationResults, true);

    if (!isValid)
        return Results.UnprocessableEntity(validationResults);

    bookService.AddBook(newBook);
    return Results.Created($"/api/books/{newBook.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces<List<ValidationResult>>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");



app.MapPut("/api/books/{id:int}", (int id, Book editBook, IBookService bookService) =>
{

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(editBook);
    var isValid = Validator.TryValidateObject(editBook, context, validationResults, true);
    if (!isValid)
        throw new ValidationException(validationResults.First().ErrorMessage);


    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

     var book = bookService.UpdateBook(id, editBook);
    
    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.Produces<ErrorDetails>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");




app.MapDelete("/api/books/{id:int}", (int id, IBookService bookService) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");


    bookService.DeleteBook(id);
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");



app.MapGet("/api/books/search", (string? title, IBookService
 bookService) =>
{
    var books = string.IsNullOrEmpty(title)
                ? bookService.GetBooks()
                : bookService?.GetBooks()?
                           .Where(b => b.Title != null
                           && b.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                           .ToList();

    return books!.Any() ? Results.Ok(books) : Results.NoContent();

})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("GETs");



app.Run();


// The book with {id} could not be found!
public abstract class NotFoundException : Exception
{
    protected NotFoundException(string message) : base(message)
    {

    }
}

public sealed class BookNotFoundException : NotFoundException
{
    public BookNotFoundException(int id) : base($"The book with {id} could not be found!")
    {
    }
}



public class ErrorDetails
{
    public int StatusCode { get; set; }
    public String? Message { get; set; }
    public String? AtOccured => DateTime.Now.ToLongDateString();
    public override string ToString() => JsonSerializer.Serialize(this);

}
public class Book
{

    [Required]
    public int Id { get; set; }

    [MinLength(2, ErrorMessage = "Min len. must be 2")]
    [MaxLength(25, ErrorMessage = "Max len. must be 25")]
    public string? Title { get; set; }

    [Range(10, 100)]
    public double Price { get; set; }
    
}

public interface IBookService
{
     int Count { get; }
     List<Book> GetBooks();
     Book? GetBookById(int id);
     void AddBook(Book book);
     Book UpdateBook(int id, Book book);
     void DeleteBook(int id);
}

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