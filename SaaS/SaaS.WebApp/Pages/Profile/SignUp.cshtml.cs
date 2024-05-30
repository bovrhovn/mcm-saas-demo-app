using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Marketplace.SaaS;
using Microsoft.Marketplace.SaaS.Models;
using Newtonsoft.Json;
using SaaS.WebApp.Data;
using SaaS.WebApp.Models;
using SaaS.WebApp.ViewModels;

namespace SaaS.WebApp.Pages.Profile;

[AllowAnonymous]
public class SignUpPageModel(
    ILogger<SignUpPageModel> logger,
    WebAppUserRepository webAppUserRepository,
    IMarketplaceSaaSClient marketplaceSaaSClient,
    IWebHostEnvironment hostEnvironment,
    PackageRepository packageRepository) : PageModel
{
    public async Task OnGet()
    {
        logger.LogInformation("Sign up page visited {DateLoaded}", DateTime.Now);
        if (!string.IsNullOrEmpty(Token))
        {
            logger.LogInformation("Token provided for signup {Token}", Token);
            if (hostEnvironment.IsDevelopment())
            {
                var decodeValue = Convert.FromBase64String(Token);
                var token = Encoding.UTF8.GetString(decodeValue);
                var subscriptionViewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(token);
                TempData["SubscriptionDev"] = subscriptionViewModel;
                SignupModel.Username = subscriptionViewModel.purchaser.emailId;
                SignupModel.FullName = subscriptionViewModel.purchaser.emailId;
            }
            else
            {
                var resolvedSubscription =
                    await marketplaceSaaSClient.Fulfillment.ResolveAsync(Token);
                if (resolvedSubscription.HasValue)
                {
                    var subscription = resolvedSubscription.Value;
                    //do the signup and align to the packages behind the scenes
                    var purchaserEmail = subscription.Subscription.Purchaser.EmailId;
                    SignupModel.FullName = purchaserEmail;
                    SignupModel.Username = purchaserEmail;
                    TempData["Subscription"] = subscription;
                }
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid signup model");
            Message = "Invalid signup - check username, password and full name fields";
            return Page();
        }

        logger.LogInformation("Signup attempt for {Email}", SignupModel.Username);
        //sign up
        try
        {
            var user = await webAppUserRepository.CreateNewUserAsync(new WebAppUser
            {
                FullName = SignupModel.FullName,
                Email = SignupModel.Username,
                Password = SignupModel.Password,
                IsAdmin = false
            });

            var planId = string.Empty;
            if (hostEnvironment.IsDevelopment())
            {
                var subscriptionViewModel = TempData["SubscriptionDev"] as SubscriptionViewModel;
                planId = subscriptionViewModel?.planId;
                logger.LogInformation("Subscription plan id {PlanId}", planId);
            }
            else
            {
                //set the subscription
                if (TempData["Subscription"] is ResolvedSubscription subscription)
                {
                    //get plan
                    planId = subscription.PlanId;
                    //activate the subscription
                    await marketplaceSaaSClient.Fulfillment.ActivateSubscriptionAsync(subscription.Id.Value,
                        new SubscriberPlan
                        {
                            PlanId = subscription.PlanId
                        });
                }
            }
            
            var package = await packageRepository.GetPackageBasedOnCodeAsync(planId);
            var selectedPlanId = package.PackageId;
            await packageRepository.SubscribeToPackageAsync(selectedPlanId, user.WebAppUserId);
            logger.LogInformation("User {UserId} subscribed to package {PackageId}", user.WebAppUserId,
                selectedPlanId);

            await HttpContext.SignInAsync(user.GenerateClaims());
            return Redirect("/");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Page();
        }
    }

    [BindProperty] public SignUpViewModel SignupModel { get; set; } = new();

    [BindProperty] public string Message { get; set; }

    [BindProperty(SupportsGet = true)] public string Token { get; set; }
}