using Microsoft.AspNetCore.Mvc;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;

namespace MVCPilotProjectWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository db)
        {
            _categoryRepository = db;
        }

        public IActionResult Index()
        {
            var categories = _categoryRepository.GetAll().ToList();
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
                _categoryRepository.Add(newCategory);
                _categoryRepository.Save();

                TempData["success"] = "Category was successfully created!";

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

            Category? categoryFromDb = _categoryRepository.Get(x=>x.Id == id);
            
            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category newCategory)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Update(newCategory);
                _categoryRepository.Save();

                TempData["success"] = "Category was successfully updated!";

                return RedirectToAction("Index");
            }

            return View(newCategory);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category? categoryFromDb = _categoryRepository.Get(x => x.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? categoryFromDb = _categoryRepository.Get(x => x.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            _categoryRepository.Remove(categoryFromDb);
            _categoryRepository.Save();

            TempData["success"] = "Category was successfully deleted!";

            return RedirectToAction("Index");
        }
    }
}
