using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;
using MVCPilotProject.Utility;

namespace MVCPilotProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var companies = _unitOfWork.Company.GetAll();

            return View(companies);
        }

        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company company = _unitOfWork.Company.Get(x => x.Id == id);
                return View(company);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Company newCompany)
        {
            if(ModelState.IsValid)
            {
                if(newCompany.Id == 0)
                {
                    _unitOfWork.Company.Add(newCompany);

                    TempData["success"] = "Company was successfully created!";
                }
                else
                {
                    _unitOfWork.Company.Update(newCompany);

                    TempData["success"] = "Company was successfully updated!";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            else
            {
                return View(newCompany);
            }
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Company? CompanyFromDb = _unitOfWork.Company.Get(x => x.Id == id);

            if (CompanyFromDb == null)
            {
                return NotFound();
            }

            _unitOfWork.Company.Remove(CompanyFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Company was successfully deleted!";

            return RedirectToAction("Index");
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var companys = _unitOfWork.Company.GetAll();

            return Json(new {data = companys});
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Company? CompanyFromDb = _unitOfWork.Company.Get(x => x.Id == id);

            if (CompanyFromDb == null)
            {
                return Json(new { success = true, data = "Error while deleting!" });
            }

            _unitOfWork.Company.Remove(CompanyFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, data = "Company was successfully deleted!" });

        }

        #endregion 
    }
}
