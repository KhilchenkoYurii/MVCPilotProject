using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCPilotProjectWebRazor.Data;
using MVCPilotProjectWebRazor.Models;

namespace MVCPilotProjectWebRazor.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _applicationDb;

        [BindProperty]
        public Category Category { get; set; }

        public DeleteModel(ApplicationDbContext applicationDb)
        {
            _applicationDb = applicationDb;
        }
        public void OnGet(int? id)
        {
            if (id != null)
            {
                Category = _applicationDb.Categories.Find(id);
            }
        }

        public IActionResult OnPost()
        {
            _applicationDb.Categories.Remove(Category);
            _applicationDb.SaveChanges();

            TempData["success"] = "Category was successfully deleted!";

            return RedirectToPage("Index");
        }
    }
}
