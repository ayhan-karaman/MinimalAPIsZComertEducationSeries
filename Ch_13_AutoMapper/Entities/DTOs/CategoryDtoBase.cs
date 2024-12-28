using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs;
public abstract record CategoryDtoBase
{

    [MinLength(2, ErrorMessage = "Category name min len. must be 2")]
    [MaxLength(15, ErrorMessage = "Category name max len. must be 15")]
    public String? CategoryName { get; init; }
}
