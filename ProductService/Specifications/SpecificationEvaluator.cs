using Microsoft.EntityFrameworkCore;

namespace ProductService.Specifications;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> specification)
    {
        var query = inputQuery;

        // WHERE
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // INCLUDE
        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        // ORDER BY
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }

        // ORDER BY DESC
        if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // PAGING
        if (specification.IsPagingEnabled)
        {
            query = query
                .Skip(specification.Skip)
                .Take(specification.Take);
        }

        return query;
    }
}