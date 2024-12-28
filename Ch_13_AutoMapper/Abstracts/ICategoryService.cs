using Entities;
using Entities.DTOs;

namespace Abstracts;
public interface ICategoryService
{
    int Count { get; }
    List<CategoryDto> GetCategories();
    CategoryDto? GetCategoryById(int id);
    Category AddCategory(CategoryDtoForInsertion category);
    Category UpdateCategory(int id, CategoryDtoForUpdate category);
    void DeleteCategory(int id);

}