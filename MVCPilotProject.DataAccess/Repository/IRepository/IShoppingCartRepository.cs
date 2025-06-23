using MVCPilotProject.Models;

namespace MVCPilotProject.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository: IRepository<ShoppingCart>
    {
        void Update(ShoppingCart category);
    }
}
