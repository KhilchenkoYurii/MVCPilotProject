using System.Linq.Expressions;

namespace MVCPilotProject.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeParameter = null);
        
        T Get(Expression<Func<T,bool>> filter, string? includeParameter = null);

        void Add(T entity);

        void Update(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}
