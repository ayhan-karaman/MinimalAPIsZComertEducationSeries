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
// Book
builder.Services.AddScoped<IBookService, BookServiceV3>();
builder.Services.AddScoped<BookRepository>();

// Category
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<CategoryRepository>();

// Database
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

/// Books 

app.MapGet("/api/books", (IBookService bookService) =>
{
    return bookService.Count > 0
        ? Results.Ok(bookService.GetBooks())
        : Results.NoContent();
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("BOOK-CRUD", "BOOK-GETs");



app.MapGet("/api/books/{id:int}", (int id, IBookService bookService) =>
{
    var book = bookService.GetBookById(id);
    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("BOOK-GETs");



app.MapPost("/api/books", (BookDtoForInsertion newBook, IBookService bookService) =>
{
    var book = bookService.AddBook(newBook);
    return Results.Created($"/api/books/{book.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces<List<ValidationResult>>(StatusCodes.Status422UnprocessableEntity)
.WithTags("BOOK-CRUD");



app.MapPut("/api/books/{id:int}", (int id, BookDtoForUpdate editBook, IBookService bookService) =>
{
     var book = bookService.UpdateBook(id, editBook);
    
    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.Produces<ErrorDetails>(StatusCodes.Status422UnprocessableEntity)
.WithTags("BOOK-CRUD");




app.MapDelete("/api/books/{id:int}", (int id, IBookService bookService) =>
{
   
    bookService.DeleteBook(id);
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("BOOK-CRUD");



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
.WithTags("BOOK-GETs");



/// Category


app.MapGet("/api/categories", (ICategoryService categoryService) =>{
     var categories = categoryService.GetCategories();
     return Results.Ok(categories);
})
.Produces<List<CategoryDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CATEGORY-CRUD", "CATEGORY-GETs");

app.MapGet("/api/categories/{id:int}", (int id, ICategoryService categoryService) =>
{
    var book = categoryService.GetCategoryById(id);
    return Results.Ok(book);
})
.Produces<Category>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CATEGORY-GETs");


app.MapPost("/api/categories", (CategoryDtoForInsertion newCategory, ICategoryService categoryService) =>
{
    var category = categoryService.AddCategory(newCategory);
    return Results.Created($"/api/books/{category.Id}", newCategory);
})
.Produces<Category>(StatusCodes.Status201Created)
.Produces<List<ValidationResult>>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CATEGORY-CRUD");



app.MapPut("/api/categories/{id:int}", (int id, CategoryDtoForUpdate editCategory, ICategoryService categoryService) =>
{
     var category = categoryService.UpdateCategory(id, editCategory);
    
    return Results.Ok(category);
})
.Produces<Category>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.Produces<ErrorDetails>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CATEGORY-CRUD");


app.MapDelete("/api/categories/{id:int}", (int id, ICategoryService categoryService) =>
{
   
    categoryService.DeleteCategory(id);
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CATEGORY-CRUD");

app.Run();



public class ErrorDetails
{
    public int StatusCode { get; set; }
    public String? Message { get; set; }
    public String? AtOccured => DateTime.Now.ToLongDateString();
    public override string ToString() => JsonSerializer.Serialize(this);

}
