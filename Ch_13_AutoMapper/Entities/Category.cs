using System.Text.Json.Serialization;

namespace Entities;
public class Category
{
    public int Id { get; set; }
    public string? CategoryName { get; set;}

    [JsonIgnore]
    public ICollection<Book>? Books { get; set; } // Collection navigation property
}