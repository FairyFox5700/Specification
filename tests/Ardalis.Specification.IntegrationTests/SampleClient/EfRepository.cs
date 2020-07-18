﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Ardalis.Specification.IntegrationTests.SampleClient
{
    /// <summary>
    /// "There's some repetition here - couldn't we have some the sync methods call the async?"
    /// https://blogs.msdn.microsoft.com/pfxteam/2012/04/13/should-i-expose-synchronous-wrappers-for-asynchronous-methods/
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EfRepository<T> where T : class, IEntity<int>
    {
        protected readonly SampleDbContext _dbContext;
        private readonly ISpecificationEvaluator<T> specificationEvaluator;

        public EfRepository(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
            this.specificationEvaluator = new SpecificationEvaluator<T>();
        }

        public EfRepository(SampleDbContext dbContext, ISpecificationEvaluator<T> specificationEvaluator)
        {
            _dbContext = dbContext;
            this.specificationEvaluator = specificationEvaluator;
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec)
        {
            {
                if (spec is null) throw new ArgumentNullException("spec is required");
                if (spec.Selector is null) throw new Exception("Specification must have Selector defined.");

                return await ApplySpecification(spec).ToListAsync();
            }
        }

        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            return specificationEvaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), specification);
        }
        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
        {
            return specificationEvaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), specification);
        }
    }
}
