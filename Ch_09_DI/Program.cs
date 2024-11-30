using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

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
builder.Services.AddSingleton<BookService>();

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



app.MapGet("/api/books", (BookService bookService) =>
{
    return bookService.GetBooks!.Count > 0
        ? Results.Ok(bookService.GetBooks)
        : Results.NoContent();
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CRUD", "GETs");



app.MapGet("/api/books/{id:int}", (int id, BookService bookService) =>
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



app.MapPost("/api/books", (Book newBook, BookService bookService) =>
{

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(newBook);
    bool isValid = Validator.TryValidateObject(newBook, context, validationResults, true);

    if (!isValid)
        return Results.UnprocessableEntity(validationResults);

    bookService.Add(newBook);
    return Results.Created($"/api/books/{newBook.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces<List<ValidationResult>>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");



app.MapPut("/api/books/{id:int}", (int id, Book editBook, BookService bookService) =>
{

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(editBook);
    var isValid = Validator.TryValidateObject(editBook, context, validationResults, true);
    if (!isValid)
        throw new ValidationException(validationResults.First().ErrorMessage);


    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

     var book = bookService.Update(id, editBook);
    
    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.Produces<ErrorDetails>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");




app.MapDelete("/api/books/{id:int}", (int id, BookService bookService) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");


    bookService.Delete(id);
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");



app.MapGet("/api/books/search", (string? title, BookService
 bookService) =>
{
    var books = string.IsNullOrEmpty(title)
                ? bookService.GetBooks
                : bookService?.GetBooks?
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



class ErrorDetails
{
    public int StatusCode { get; set; }
    public String? Message { get; set; }
    public String? AtOccured => DateTime.Now.ToLongDateString();
    public override string ToString() => JsonSerializer.Serialize(this);

}
class Book
{

    [Required]
    public int Id { get; set; }

    [MinLength(2, ErrorMessage = "Min len. must be 2")]
    [MaxLength(25, ErrorMessage = "Max len. must be 25")]
    public string? Title { get; set; }

    [Range(10, 100)]
    public double Price { get; set; }
    
}

class BookService
{
    // readonly sınıf üyeleri sadece okunabilir sınıf üyeleridir. 
    // readonly sınıf üyelerinin instance'sı yapıcı metot veya tanımlandığı yerde üretilebilir.
    private readonly List<Book> _bookList;
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

    public List<Book>? GetBooks => _bookList;

    public Book? GetBookById(int id) => 
        _bookList.FirstOrDefault(b => b.Id.Equals(id));

    public void Add(Book newBook)
    {
         newBook.Id = _bookList.Max(b => b.Id) + 1;
         _bookList.Add(newBook);
    }

    public Book Update(int id, Book editBook)
    {
         Book? book = _bookList.FirstOrDefault(b => b.Id.Equals(id));
         if(book is null)
            throw new BookNotFoundException(id);
            
         book.Title = editBook.Title;
         book.Price = editBook.Price;
         return book;
    }

    public void Delete(int id)
    {
        Book? book = _bookList.FirstOrDefault(b => b.Id.Equals(id));
        if(book is not null)
            _bookList.Remove(book);
        else
            throw new BookNotFoundException(id);
    }
}