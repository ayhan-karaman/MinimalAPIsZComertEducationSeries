
using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs;
public abstract record BookDto
{

        [MinLength(2, ErrorMessage = "Min len. must be 2")]
        [MaxLength(25, ErrorMessage = "Max len. must be 25")]
        public String Title { get; init; }

        [Range(10, 100, ErrorMessage ="Price must be between 1 and 100.")]
        public Decimal Price { get; set; }
}
