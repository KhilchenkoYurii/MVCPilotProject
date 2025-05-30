using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCPilotProjectWebRazor.Data;
using MVCPilotProjectWebRazor.Models;

namespace MVCPilotProjectWebRazor.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _applicationDb;

        public List<Category> Categories { get; set; }

        public IndexModel(ApplicationDbContext applicationDb)
        {
            _applicationDb = applicationDb;
        }

        public void OnGet()
        {
            Categories = _applicationDb.Categories.ToList();
        }
    }
}
