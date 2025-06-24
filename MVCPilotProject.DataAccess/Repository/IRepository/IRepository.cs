using System.Linq.Expressions;

namespace MVCPilotProject.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeParameter = null);
        
        T Get(Expression<Func<T,bool>> filter, string? includeParameter = null, bool tracked = false);

        void Add(T entity);

        void Update(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}
