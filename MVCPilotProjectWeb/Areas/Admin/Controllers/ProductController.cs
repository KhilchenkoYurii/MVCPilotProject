using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;

namespace MVCPilotProjectWeb.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll().ToList();

            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem()
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product newProduct)
        {
            if (newProduct.Price <= 0 || newProduct.Price50 <=0 || newProduct.Price100<=0)
            {
                ModelState.AddModelError("price", "Any price should be grater than zero");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(newProduct);
                _unitOfWork.Save();

                TempData["success"] = "Product was successfully created!";

                return RedirectToAction("Index");
            }

            return View(newProduct);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product? ProductFromDb = _unitOfWork.Product.Get(x => x.Id == id);

            if (ProductFromDb == null)
            {
                return NotFound();
            }

            return View(ProductFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Product newProduct)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(newProduct);
                _unitOfWork.Save();

                TempData["success"] = "Product was successfully updated!";

                return RedirectToAction("Index");
            }

            return View(newProduct);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product? ProductFromDb = _unitOfWork.Product.Get(x => x.Id == id);

            if (ProductFromDb == null)
            {
                return NotFound();
            }

            return View(ProductFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? ProductFromDb = _unitOfWork.Product.Get(x => x.Id == id);

            if (ProductFromDb == null)
            {
                return NotFound();
            }

            _unitOfWork.Product.Remove(ProductFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Product was successfully deleted!";

            return RedirectToAction("Index");
        }
    }
}
