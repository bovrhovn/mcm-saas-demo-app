using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SaaS.WebApp.Pages.Info;

public class IndexPageModel(ILogger<IndexPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("Index page visited at {DateLoaded}", DateTime.Now);
}