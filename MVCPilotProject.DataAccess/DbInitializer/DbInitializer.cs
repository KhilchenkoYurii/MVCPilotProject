using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCPilotProject.DataAccess.Data;
using MVCPilotProject.Models;
using MVCPilotProject.Utility;

namespace MVCPilotProject.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

            
        public DbInitializer(UserManager<IdentityUser> user, RoleManager<IdentityRole> role, ApplicationDbContext db) 
        {
            _userManager = user;
            _roleManager = role;
            _db = db;
        }
        public void Initialize()
        {
            //migrations if they are not
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            { 
            }
            //create roles if they are not
            if (!_roleManager.RoleExistsAsync(SD.RoleCustomer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.RoleCustomer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleEmployee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleCompany)).GetAwaiter().GetResult();

                //if roles not create, than create an admin user
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@dotnet.com",
                    Email = "admin@dotnet.com",
                    Name = "admin",
                    PhoneNumber = "1234567890",
                    StreetAddress = "test address",
                    State = "IL",
                    PostalCode = "2488",
                    City = "Test city"
                },"Admin12345!").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.Equals("admin@dotnet.com"));
                _userManager.AddToRoleAsync(user, SD.RoleAdmin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
