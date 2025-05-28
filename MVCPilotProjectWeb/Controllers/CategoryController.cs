using Microsoft.AspNetCore.Mvc;
using MVCPilotProjectWeb.Data;
using MVCPilotProjectWeb.Models;

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category newCategory)
        {
            if (newCategory.Name == newCategory.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order can't match with name");
            }

            if (ModelState.IsValid)
            {
                _applicationDb.Add(newCategory);
                _applicationDb.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(newCategory);
        }

        public IActionResult Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Category? categoryFromDb = _applicationDb.Categories.Find(id);
            
            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category newCategory)
        {
            if (newCategory.Name == newCategory.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order can't match with name");
            }

            if (ModelState.IsValid)
            {
                _applicationDb.Add(newCategory);
                _applicationDb.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(newCategory);
        }
    }
}
