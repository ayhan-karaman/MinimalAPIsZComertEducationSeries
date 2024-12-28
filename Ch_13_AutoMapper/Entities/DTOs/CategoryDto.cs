namespace Entities.DTOs;

public record CategoryDto : CategoryDtoBase
{
    public int Id { get; init; }
    public List<Book>? Books { get; init; }
}
