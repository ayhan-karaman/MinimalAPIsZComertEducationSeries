using System.ComponentModel.DataAnnotations;
namespace Entities;
public class Book
{
    public int Id { get; set; }

    
    public string? Title { get; set; }

    
    public double Price { get; set; }
    
}
