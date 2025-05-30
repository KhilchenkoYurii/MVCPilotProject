using Microsoft.EntityFrameworkCore;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;
using MVCPilotProjectWeb.DataAcess.Data;

namespace MVCPilotProject.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
