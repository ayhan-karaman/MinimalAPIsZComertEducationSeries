using AutoMapper;
using Entities;
using Entities.DTOs;

namespace Configuration;
public class MappingProfile:Profile
{
    public MappingProfile()
    {
        CreateMap<BookDtoForInsertion, Book>().ReverseMap();
        CreateMap<BookDtoForUpdate, Book>().ReverseMap();
        CreateMap<Book, BookDto>().ReverseMap();

        CreateMap<CategoryDtoForInsertion, Category>().ReverseMap();
        CreateMap<CategoryDtoForUpdate, Category>().ReverseMap();
        CreateMap<Category, CategoryDto>().ReverseMap();
    }
}