using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
     c.SwaggerDoc("v1", new(){
           Title = "Book API",
           Version = "V1",
           Description = "Virtual campus minimal api training program",
           License = new (),
           TermsOfService = new("https://www.samsun.edu.tr"),
           Contact = new(){
              Email = "example@example.com.tr",
              Name = "Ayhan Karaman",
              Url = new("https://www.youtube.com/@virtual.campus")
           }
     });
});

builder.Services.AddCors(options => {
     options.AddPolicy("all", builder => {
          builder.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
     });

     options.AddPolicy("special", builder =>{
           builder.WithOrigins("https://localhost:300")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
     });
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

app.UseExceptionHandler(appError => {
    appError.Run(async context => {
        
         context.Response.StatusCode = StatusCodes.Status500InternalServerError;
         context.Response.ContentType = "application/json";

         var contextFeatures = context.Features.Get<IExceptionHandlerFeature>();

         if (contextFeatures is not null)
         {
            context.Response.StatusCode = contextFeatures.Error switch 
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsync(
                    new ErrorDetails(){
                         Message = contextFeatures.Error.Message,
                         StatusCode = context.Response.StatusCode
                    }.ToString()
             );
         }
    });
});

app.MapGet("/api/errors", () => {
    throw new Exception("An error has been occured.");
})
.Produces<ErrorDetails>(StatusCodes.Status500InternalServerError)
.ExcludeFromDescription();



app.MapGet("/api/books", () => {
    return Book.List.Count > 0
        ? Results.Ok(Book.List)
        : Results.NoContent();
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CRUD", "GETs");

app.MapGet("/api/books/{id:int}", (int id) => {
    
    if(!(id > 0 && id <= 1000))
         throw new ArgumentOutOfRangeException("1-1000");

     var book = Book.List.FirstOrDefault(b => b.Id.Equals(id));
      if(book is not null) 
        return Results.Ok(book);
    throw new BookNotFoundException(id); //Results.NotFound();
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("GETs");

app.MapPost("/api/books", (Book newBook) => {
    newBook.Id = Book.List.Select(b => b.Id).Max() + 1;
    Book.List.Add(newBook);
    return Results.Created($"/api/books/{newBook.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.WithTags("CRUD");

app.MapPut("/api/books/{id:int}", (int id, Book editBook) => {

    if(!(id > 0 && id <= 1000))
         throw new ArgumentOutOfRangeException("1-1000");

    var book = Book.List.FirstOrDefault(b => b.Id.Equals(id));
    if (book is null)
         throw new BookNotFoundException(id);
    book.Title = editBook.Title;
    book.Price = editBook.Price;
    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");

app.MapDelete("/api/books/{id:int}", (int id) => {
    if(!(id > 0 && id <= 1000))
         throw new ArgumentOutOfRangeException("1-1000");

    var book = Book.List.FirstOrDefault(b => b.Id.Equals(id));
    if(book is null)
        throw new BookNotFoundException(id);
    Book.List.Remove(book);
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");

app.MapGet("/api/books/search", (string? title) => {
     var books = string.IsNullOrEmpty(title)  
                 ? Book.List
                 : Book.List
                            .Where(b => b.Title != null 
                            && b.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                            .ToList();

      return books.Any() ? Results.Ok(books) : Results.NoContent();

})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("GETs");

app.Run();


// The book with {id} could not be found!
public abstract class NotFoundException : Exception
{
    protected NotFoundException(string message):base(message)
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
    public int Id { get; set; }
    public string? Title { get; set; }
    public double Price { get; set; }

    private static List<Book> _bookList = new List<Book>()
    {
        new Book(){Id = 1, Title = "Deniz Feneri", Price=84.40},
        new Book(){Id = 2, Title = "Sol Ayağım", Price=159.00},
        new Book(){Id = 3, Title = "Simyacı", Price=190.00}

        // Şeker Portakalı - 190.45
    };

    public static List<Book> List => _bookList;
}