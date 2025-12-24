using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BookStore.DataAccess.Repository
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;

        private DbSet<T> dbset;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbset = _db.Set<T>();
        }
        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public void Delete(T entity)
        {
            dbset.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
                dbset.RemoveRange(entities);
        }

        public T Get(Expression<Func<T, bool>> predicate,string? includeprops = null)
        {
            IQueryable<T> query = dbset;
           
            query = query.Where(predicate);
            if (!string.IsNullOrEmpty(includeprops))
            {
                foreach (var includeprop in includeprops.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            return query.FirstOrDefault()!;
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate, string? includeprops = null)
        {
            IQueryable<T> query = dbset;
            if(predicate is not null)
            {
                query = query.Where(predicate);
            }
            if(!string.IsNullOrEmpty(includeprops))
            {
                foreach (var includeprop in includeprops.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            return query.ToList();
        }
    }
}
