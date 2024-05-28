using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.Data;
using SaaS.WebApp.Models;

namespace SaaS.WebApp.Pages.Profile;

public class DashboardPageModel(
    ILogger<DashboardPageModel> logger,
    UserDataContext userDataContext,
    WebAppUserRepository webAppUserRepository) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var user = userDataContext.GetCurrentUser();
        if (!user.IsAdmin)
        {
            logger.LogInformation("User {UserId} tried to access the index page without permission", user.UserId);
            return Redirect("/NoAccess");
        }

        WebAppUsersList = await webAppUserRepository.GetUsersAsync();
        logger.LogInformation("WebApp users list loaded successfully. {UsersCount} users found", WebAppUsersList.Count);
        return Page();
    }

    [BindProperty] public List<WebAppUser> WebAppUsersList { get; set; }
}