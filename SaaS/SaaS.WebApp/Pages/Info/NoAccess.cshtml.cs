using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SaaS.WebApp.Pages.Info;

public class NoAccessPageModel(ILogger<NoAccessPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("NoAccess page visited at {DateLoaded}", DateTime.Now);
}