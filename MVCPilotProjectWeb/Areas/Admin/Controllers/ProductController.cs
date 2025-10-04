using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;
using MVCPilotProject.Models.ViewModels;
using MVCPilotProject.Utility;

namespace MVCPilotProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
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
                productVM.Product = _unitOfWork.Product.Get(x => x.Id == id, includeParameter:"ProductImages");
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM newProduct, List<IFormFile>? files)
        {
            if (newProduct.Product.Price <= 0 || newProduct.Product.Price50 <= 0 || newProduct.Product.Price100 <= 0)
            {
                ModelState.AddModelError("price", "Any price should be grater than zero");
            }

            if (ModelState.IsValid)
            {
                if (newProduct.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(newProduct.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(newProduct.Product);
                }

                _unitOfWork.Save();

                string wwwRootPath = _webHost.WebRootPath;

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                        string productPath = @"images\products\product-" + newProduct.Product.Id;

                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new ProductImage()
                        {
                            ImageUrl = $"\\{productPath}\\{fileName}",
                            ProductId = newProduct.Product.Id,
                        };

                        if(newProduct.Product.ProductImages == null)
                        {
                            newProduct.Product.ProductImages = new List<ProductImage>();
                        }

                        newProduct.Product.ProductImages.Add(productImage);
                    }

                    _unitOfWork.Product.Update(newProduct.Product);
                    _unitOfWork.Save();
                }

                TempData["success"] = "Product was successfully create/updated!";

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

        public IActionResult DeleteImage(int imageid)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(x => x.Id.Equals(imageid));

            if(imageToBeDeleted != null)
            {
                if(!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImage = Path.Combine(_webHost.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Image was successfully deleted!";

            }

            return RedirectToAction(nameof(Upsert), new { id = imageToBeDeleted.ProductId });
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

            string productPath = @"images\products\product-" + id;

            string finalPath = Path.Combine(_webHost.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);

                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }

            _unitOfWork.Product.Remove(ProductFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, data = "Product was successfully deleted!" });

        }

        #endregion 
    }
}
