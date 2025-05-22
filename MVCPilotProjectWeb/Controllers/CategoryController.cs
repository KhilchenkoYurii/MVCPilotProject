using Microsoft.AspNetCore.Mvc;
using MVCPilotProjectWeb.Data;

namespace MVCPilotProjectWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _applicationDb;

        public CategoryController(ApplicationDbContext applicationDb)
        {
            _applicationDb = applicationDb;
        }

        public IActionResult Index()
        {
            var categories = _applicationDb.Categories.ToList();
            return View(categories);
        }
    }
}
