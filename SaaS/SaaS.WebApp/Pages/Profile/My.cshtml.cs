using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.Data;
using SaaS.WebApp.Models;

namespace SaaS.WebApp.Pages.Profile;

[Authorize]
public class MyPageModel(
    ILogger<MyPageModel> logger,
    UserDataContext userDataContext,
    WebAppUserRepository webAppUserRepository,
    PackageRepository packageRepository) : PageModel
{
    public async Task OnGetAsync()
    {
        logger.LogInformation("Profile page visited at {DateLoaded}", DateTime.Now);
        var user = userDataContext.GetCurrentUser();
        CurrentUser = await webAppUserRepository.GetDetailsForUserAsync(user.UserId);
        logger.LogInformation("User {UserId} details loaded", CurrentUser.WebAppUserId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = userDataContext.GetCurrentUser();
        try
        {
            var form = await Request.ReadFormAsync();
            var packageId = int.Parse(form["packageId"]);
            await packageRepository.UnSubscribeFromPackageAsync(packageId, currentUser.UserId);
            logger.LogInformation("User {UserId} unsubscribed from package {PackageId}", currentUser.UserId, packageId);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Page();
        }

        return RedirectToPage("/Profile/MyPackages");
    }

    [BindProperty] public WebAppUser CurrentUser { get; set; }
}