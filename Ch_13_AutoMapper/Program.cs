using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Abstracts;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;
using Entities;
using Entities.DTOs;
using Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddCustomCors();

// DI registration
builder.Services.AddScoped<IBookService, BookServiceV3>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddDbContext<RepositoryContext>(options => {
     options.UseSqlite(builder.Configuration.GetConnectionString("sqlite"));
});

builder.Services.AddAutoMapper(typeof(Program));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("all");
app.UseHttpsRedirection();

app.UseCustomExceptionHandler();

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
    var book = bookService.GetBookById(id);
    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("GETs");



app.MapPost("/api/books", (BookDtoForInsertion newBook, IBookService bookService) =>
{
    var book = bookService.AddBook(newBook);
    return Results.Created($"/api/books/{book.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces<List<ValidationResult>>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");



app.MapPut("/api/books/{id:int}", (int id, BookDtoForUpdate editBook, IBookService bookService) =>
{
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



public class ErrorDetails
{
    public int StatusCode { get; set; }
    public String? Message { get; set; }
    public String? AtOccured => DateTime.Now.ToLongDateString();
    public override string ToString() => JsonSerializer.Serialize(this);

}
