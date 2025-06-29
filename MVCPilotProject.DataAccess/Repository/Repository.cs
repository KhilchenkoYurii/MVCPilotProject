﻿using Microsoft.EntityFrameworkCore;
using MVCPilotProject.DataAccess.Data;
using MVCPilotProject.DataAccess.Repository.IRepository;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace MVCPilotProject.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;

        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();

            _db.Products.Include(u => u.Category).Include(u => u.CategoryId);
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeParameter, bool tracked = false)
        {
            IQueryable<T> query;

            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            if (!string.IsNullOrEmpty(includeParameter))
            {
                foreach (var parameter in includeParameter
                             .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(parameter);
                }
            }

            query = query.Where(filter);

            return query.FirstOrDefault();
        }
           

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeParameter = null)
        {
            
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);

            }

            if (!string.IsNullOrEmpty(includeParameter))
            {
                foreach (var parameter in includeParameter
                             .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(parameter);
                }
            }

            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }
    }

}
