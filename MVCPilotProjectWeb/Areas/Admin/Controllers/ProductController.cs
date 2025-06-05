using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;
using MVCPilotProject.Models.ViewModels;

namespace MVCPilotProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHost;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHost = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll(includeParameter:"Category");

            return View(products);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new() {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem()
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(x => x.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM newProduct, IFormFile? file)
        {
            if (newProduct.Product.Price <= 0 || newProduct.Product.Price50 <= 0 || newProduct.Product.Price100 <= 0)
            {
                ModelState.AddModelError("price", "Any price should be grater than zero");
            }

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHost.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(newProduct.Product.ImageUrl))
                    {
                        var oldImage = Path.Combine(wwwRootPath, newProduct.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }


                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    newProduct.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if(newProduct.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(newProduct.Product);

                    TempData["success"] = "Product was successfully created!";
                }
                else
                {
                    _unitOfWork.Product.Update(newProduct.Product);

                    TempData["success"] = "Product was successfully updated!";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            else
            {
                newProduct.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem()
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });

                return View(newProduct);
            }
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

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitOfWork.Product.GetAll(includeParameter: "Category");

            return Json(new {data = products});
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Product? ProductFromDb = _unitOfWork.Product.Get(x => x.Id == id);

            if (ProductFromDb == null)
            {
                return Json(new { success = true, data = "Error while deleting!" });
            }

            var oldImage = Path.Combine(_webHost.WebRootPath, ProductFromDb.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }

            _unitOfWork.Product.Remove(ProductFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, data = "Product was successfully deleted!" });

        }

        #endregion 
    }
}
