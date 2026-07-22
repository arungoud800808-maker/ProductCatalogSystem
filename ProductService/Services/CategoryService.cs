using AutoMapper;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category == null)
            return null;

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);

        await _repository.AddAsync(category);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category == null)
            return null;

        category.Name = dto.Name;

        await _repository.UpdateAsync(category);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category != null)
        {
            await _repository.DeleteAsync(category);
        }
    }
}