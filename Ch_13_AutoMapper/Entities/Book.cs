using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Entities;
public class Book
{
    public int Id { get; set; }
    public int CategoryId  { get; set; } // Navigation property
    
    public string? Title { get; set; }

    
    public double Price { get; set; }

    public string? URL { get; set; }

    [JsonIgnore]
    public Category? Category { get; set; }
    public Book()
    {
        URL = "/images/default.jpg";
    }
}
