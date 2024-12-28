using Abstracts;
using AutoMapper;
using Configuration;
using Entities;
using Entities.DTOs;
using Entities.Exceptions;
using Repositories;

namespace Services;
public class CategoryService : ICategoryService
{
    private readonly CategoryRepository _categoryRepo;
    private readonly IMapper _mapper;

    public CategoryService(CategoryRepository categoryRepo, IMapper mapper)
    {
        _categoryRepo = categoryRepo;
        _mapper = mapper;
    }

    public int Count => _categoryRepo.GetAll().Count;

    public Category AddCategory(CategoryDtoForInsertion category)
    {
        category.Validate();
        var mappingCategory = _mapper.Map<Category>(category);
        _categoryRepo.Add(mappingCategory);
        return mappingCategory;
    }

    public void DeleteCategory(int id)
    {
        id.ValidationIdInRange();
        var category = _categoryRepo.Get(id);
        if (category != null)
        {
            _categoryRepo.Remove(category);
        }
        else
        {
            throw new CategoryNotFoundException(id);
        }
    }

    public List<CategoryDto> GetCategories()
    {
        var categories = _categoryRepo.GetAll();
         return _mapper.Map<List<CategoryDto>>(categories);
    }

    public CategoryDto? GetCategoryById(int id)
    {
        id.ValidationIdInRange();
        var category = _categoryRepo.Get(id);
        if (category == null)
            throw new CategoryNotFoundException(id);
        return _mapper.Map<CategoryDto>(category);
    }

    public Category UpdateCategory(int id, CategoryDtoForUpdate category)
    {
        id.ValidationIdInRange();
        category.Validate();
        var mappingCategory = _categoryRepo.Get(id);
        if(mappingCategory == null)
            throw new CategoryNotFoundException(id);
        
        mappingCategory = _mapper.Map(category, mappingCategory);
        _categoryRepo.Update(mappingCategory!);
        return mappingCategory!;
    }

    
}