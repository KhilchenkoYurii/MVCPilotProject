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
        private readonly ApplicationDbContext _applicationDb;

        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext applicationDb, UserManager<IdentityUser> userManager)
        {
            _applicationDb = applicationDb;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            var roleId = _applicationDb.UserRoles.FirstOrDefault(u => u.UserId.Equals(userId)).RoleId;

            RoleManagementVM roleVm = new RoleManagementVM()
            {
                ApplicationUser = _applicationDb.ApplicationUsers.Include(u=>u.Company).FirstOrDefault(u => u.Id.Equals(userId)),

                RoleList = _applicationDb.Roles.Select(i=>new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),

                CompanyList = _applicationDb.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            roleVm.ApplicationUser.Role = _applicationDb.Roles.FirstOrDefault(u => u.Id.Equals(roleId)).Name;

            return View(roleVm);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleVm)
        {
            var roleId = _applicationDb.UserRoles.FirstOrDefault(u => u.UserId.Equals(roleVm.ApplicationUser.Id)).RoleId;

            var oldRole = _applicationDb.Roles.FirstOrDefault(u => u.Id.Equals(roleId)).Name;

            if (!roleVm.ApplicationUser.Role.Equals(oldRole))
            {
                ApplicationUser applicationUser = _applicationDb.ApplicationUsers.FirstOrDefault(u => u.Id.Equals(roleVm.ApplicationUser.Id));
                if (roleVm.ApplicationUser.Role.Equals(SD.RoleCompany))
                {
                    applicationUser.CompanyId = roleVm.ApplicationUser.CompanyId;
                }
                if (oldRole.Equals(SD.RoleCompany))
                {
                    applicationUser.CompanyId = null;
                }
                _applicationDb.SaveChanges();

                _userManager.RemoveFromRoleAsync(applicationUser,oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser,roleVm.ApplicationUser.Role).GetAwaiter().GetResult();
            }

            return RedirectToAction("Index");
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> users = _applicationDb.ApplicationUsers.Include(u=>u.Company).ToList();

            var userRoles = _applicationDb.UserRoles.ToList();

            var roles = _applicationDb.Roles.ToList();

            foreach (var user in users)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId.Equals(user.Id)).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id.Equals(roleId)).Name;

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
            var user = _applicationDb.ApplicationUsers.FirstOrDefault(u => u.Id.Equals(id));

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

            _applicationDb.SaveChanges();

            return Json(new { success = true, message = "User successfully locked/unlocked!" });
        }

        #endregion 
    }
}
