using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.Data;
using SaaS.WebApp.Models;

namespace SaaS.WebApp.Pages.Profile;

public class DashboardPageModel(
    ILogger<DashboardPageModel> logger,
    UserDataContext userDataContext,
    WebAppUserRepository webAppUserRepository,
    PackageRepository packageRepository) : PageModel
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

    public async Task<IActionResult> OnPostUnsubscribeAsync(Package package)
    {
        var user = userDataContext.GetCurrentUser();
        var result = await packageRepository.UnSubscribeFromPackageAsync(package.PackageId, user.UserId);
        logger.LogInformation("User {UserId} unsubscribed from package {PackageId}. Result: {Result}", user.UserId,
            package.PackageId, result);
        return new JsonResult(result);
    }
}