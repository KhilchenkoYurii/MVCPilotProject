﻿using Microsoft.EntityFrameworkCore;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProjectWeb.DataAcess.Data;
using System.Linq.Expressions;


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
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> queryable = dbSet;

            queryable = queryable.Where(filter);

            return queryable.FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> queryable = dbSet;

            return queryable.ToList();
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
