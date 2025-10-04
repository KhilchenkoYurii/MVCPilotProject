using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCPilotProject.DataAccess.Data;
using MVCPilotProject.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCPilotProject.Models;
using MVCPilotProject.Models.ViewModels;
using MVCPilotProject.Utility;
using Microsoft.AspNetCore.Identity;

namespace MVCPilotProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class UserController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IUnitOfWork _unitOfWork;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM roleVm = new RoleManagementVM()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u=>u.Id.Equals(userId), includeParameter:"Company"),

                RoleList = _roleManager.Roles.Select(i=>new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),

                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            roleVm.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id.Equals(userId))).GetAwaiter().GetResult().FirstOrDefault();

            return View(roleVm);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleVm)
        {
            var oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id.Equals(roleVm.ApplicationUser.Id))).GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id.Equals(roleVm.ApplicationUser.Id));

            if (!roleVm.ApplicationUser.Role.Equals(oldRole))
            {
                if (roleVm.ApplicationUser.Role.Equals(SD.RoleCompany))
                {
                    applicationUser.CompanyId = roleVm.ApplicationUser.CompanyId;
                }
                if (oldRole.Equals(SD.RoleCompany))
                {
                    applicationUser.CompanyId = null;
                }

                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser,oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser,roleVm.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole.Equals(SD.RoleCompany) && !applicationUser.CompanyId.Equals(roleVm.ApplicationUser.Company))
                {
                    applicationUser.CompanyId = roleVm.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }
                return RedirectToAction("Index");
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> users = _unitOfWork.ApplicationUser.GetAll(includeParameter:"Company").ToList();

            foreach (var user in users)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                if (user.Company == null)
                {
                    user.Company = new Company { Name = string.Empty };
                }
            }

            return Json(new {data = users });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id.Equals(id));

            if(user == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking!" });

            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.ApplicationUser.Update(user);
            _unitOfWork.Save();

            return Json(new { success = true, message = "User successfully locked/unlocked!" });
        }

        #endregion 
    }
}
