using E_Commerce.Domain.Common;
using E_Commerce.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Infrastructure.Specifications
{
    internal static class SpecificationEvaluator
    {
        public static IQueryable<TEntity> CreateQuery<TEntity, TKey>(IQueryable<TEntity> inputQuery, ISpecifications<TEntity, TKey> specifications) where TEntity : BaseEntity<TKey>
        {
            var query = inputQuery;

            if (specifications.Criteria != null)
            {
                query = query.Where(specifications.Criteria);
            }


            if (specifications.IncludeExpressions.Any())
            {
                query = specifications.IncludeExpressions.Aggregate(query, (current, NextExp) => current.Include(NextExp));
            }


            if (specifications.OrderBy is not null)
            {
                query = query.OrderBy(specifications.OrderBy);
            }


            if (specifications.OrderByDescending is not null)
            {
                query = query.OrderByDescending(specifications.OrderByDescending);
            }


            if (!specifications.IsTrackingEnabled)
            {
                query = query.AsNoTracking();
            }


            if (specifications.IsPaginated)
            {
                query = query.Skip(specifications.Skip).Take(specifications.Take);
            }

            return query;
        }
    }
}
