using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Helpers;
using BLL.DTOs;

namespace PRN222_FinalProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public UserDto? CurrentUser { get; set; }

        public void OnGet()
        {
            CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        }
    }
}
