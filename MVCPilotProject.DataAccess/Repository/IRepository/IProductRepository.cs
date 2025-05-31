using MVCPilotProject.Models;

namespace MVCPilotProject.DataAccess.Repository.IRepository
{
    public interface IProductRepository: IRepository<Product>
    {
        void Update(Product product);
    }
}
