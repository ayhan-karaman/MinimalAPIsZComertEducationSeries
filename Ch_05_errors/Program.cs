using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
});



app.MapGet("/api/books", () => Book.List);

app.MapGet("/api/books/{id:int}", (int id) => {
     var book = Book.List.FirstOrDefault(b => b.Id.Equals(id));
      if(book is not null) 
        return Results.Ok(book);
    throw new BookNotFoundException(id); //Results.NotFound();
});

app.MapPost("/api/books", (Book newBook) => {
    newBook.Id = Book.List.Select(b => b.Id).Max() + 1;
    Book.List.Add(newBook);
    return Results.Created($"/api/books/{newBook.Id}", newBook);
});

app.MapPut("/api/books/{id:int}", (int id, Book editBook) => {
    var book = Book.List.FirstOrDefault(b => b.Id.Equals(id));
    if (book is null)
        return Results.NotFound();
    book.Title = editBook.Title;
    book.Price = editBook.Price;
    return Results.Ok(book);
});

app.MapDelete("/api/books/{id:int}", (int id) => {
    var book = Book.List.FirstOrDefault(b => b.Id.Equals(id));
    if(book is null)
        return Results.NotFound();
    Book.List.Remove(book);
    return Results.NoContent();
});

app.MapGet("/api/books/search", (string? title) => {
     var books = string.IsNullOrEmpty(title)  
                 ? Book.List
                 : Book.List
                            .Where(b => b.Title != null 
                            && b.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                            .ToList();

      return books.Any() ? Results.Ok(books) : Results.NoContent();

});

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