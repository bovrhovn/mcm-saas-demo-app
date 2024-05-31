using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.Data;

namespace SaaS.WebApp.Pages.Profile;

public class DashboardPageModel(
    ILogger<DashboardPageModel> logger,
    UserDataContext userDataContext,
    WebAppUserRepository webAppUserRepository) : PageModel
{
    public IActionResult OnGet()
    {
        var user = userDataContext.GetCurrentUser();
        if (user.IsAdmin) return Page();
        logger.LogInformation("User {UserId} tried to access the index page without permission", user.UserId);
        return RedirectToPage("/Info/NoAccess");
    }

    public async Task<IActionResult> OnGetSearchAsync(string query)
    {
        logger.LogInformation("Search for {Search} initiated", query);
        var list = await webAppUserRepository.GetUsersAsync(query);
        logger.LogInformation("Search for {Search} completed. {UserCount} packages found", query, list.Count);
        return new JsonResult(list);
    }
}