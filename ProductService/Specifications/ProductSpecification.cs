using ProductService.Models;

namespace ProductService.Specifications;

public class ProductSpecification : BaseSpecification<Product>
{
    // Get all products
    public ProductSpecification()
    {
        AddInclude(p => p.Category);

        AddOrderBy(p => p.Id);
    }

    // Get single product
    public ProductSpecification(int id)
        : base(p => p.Id == id)
    {
        AddInclude(p => p.Category);
    }

    // Search + Sort + Paging
    public ProductSpecification(
        string? search,
        string? sort,
        int pageNumber,
        int pageSize)
    {
        AddInclude(p => p.Category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            Criteria = p =>
                p.Name.Contains(search) ||
                p.Description.Contains(search);
        }

        switch (sort?.ToLower())
        {
            case "price":
                AddOrderBy(p => p.Price);
                break;

            case "pricedesc":
                AddOrderByDescending(p => p.Price);
                break;

            case "name":
                AddOrderBy(p => p.Name);
                break;

            case "stock":
                AddOrderBy(p => p.Stock);
                break;

            default:
                AddOrderBy(p => p.Id);
                break;
        }

        ApplyPaging(
            (pageNumber - 1) * pageSize,
            pageSize);
    }
}