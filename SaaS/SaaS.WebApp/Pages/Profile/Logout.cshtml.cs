using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.Data;

namespace SaaS.WebApp.Pages.Profile;

[Authorize]
public class LogoutPageModel(ILogger<LogoutPageModel> logger, UserDataContext dataContext) : PageModel
{
    public void OnGet()
    {
        var user = dataContext.GetCurrentUser();
        logger.LogInformation("Logout page visited {DateLoaded} with user {FullName}", DateTime.Now, user.Fullname);
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var user = dataContext.GetCurrentUser();
        logger.LogInformation("Logout for {FullName}", user.Fullname);
        await dataContext.LogOut();
        return Redirect("/");
    }
}