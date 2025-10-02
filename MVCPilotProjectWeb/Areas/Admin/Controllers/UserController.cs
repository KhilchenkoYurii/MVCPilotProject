using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCPilotProject.DataAccess.Data;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;
using MVCPilotProject.Utility;

namespace MVCPilotProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _applicationDb;

        public UserController(ApplicationDbContext applicationDb)
        {
            _applicationDb = applicationDb;
        }

        public IActionResult Index()
        {
            return View();
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
