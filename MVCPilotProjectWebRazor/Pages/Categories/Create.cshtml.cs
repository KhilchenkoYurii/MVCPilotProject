using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCPilotProjectWebRazor.Data;
using MVCPilotProjectWebRazor.Models;

namespace MVCPilotProjectWebRazor.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _applicationDb;

        [BindProperty]
        public Category Category { get; set; }
        
        public CreateModel(ApplicationDbContext applicationDb)
        {
            _applicationDb = applicationDb;
        }
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            _applicationDb.Categories.Add(Category);
            _applicationDb.SaveChanges();

            TempData["success"] = "Category was successfully updated!";

            return RedirectToPage("Index");
        }
    }
}
