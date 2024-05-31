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
    public async Task<IActionResult> OnGet()
    {
        logger.LogInformation("Sign up page visited {DateLoaded}", DateTime.Now);
        if (User.Identity is { IsAuthenticated: true })
        {
            logger.LogInformation("User already authenticated {UserId}", User.Identity.Name);
            return Page();
        }

        if (string.IsNullOrEmpty(Token))
        {
            logger.LogInformation("Normal signup page loaded (without token)");
            return Page();
        }

        logger.LogInformation("Token provided for signup {Token}", Token);
        if (hostEnvironment.IsDevelopment())
        {
            var decodeValue = Convert.FromBase64String(Token);
            var token = Encoding.UTF8.GetString(decodeValue);
            var subscriptionViewModel = JsonConvert.DeserializeObject<SubscriptionViewModel>(token);
            TempData["SubscriptionDev"] = token;
            SignupModel = new SignUpViewModel
            {
                Username = subscriptionViewModel.purchaser.emailId,
                FullName = subscriptionViewModel.purchaser.emailId
            };
        }
        else
        {
            var resolvedSubscription =
                await marketplaceSaaSClient.Fulfillment.ResolveAsync(Token);
            if (!resolvedSubscription.HasValue) return Page();
            
            var subscription = resolvedSubscription.Value;
            //do the signup and align to the packages behind the scenes
            var purchaserEmail = subscription.Subscription.Purchaser.EmailId;
            SignupModel.FullName = purchaserEmail;
            SignupModel.Username = purchaserEmail;
            var passedData = JsonConvert.SerializeObject(subscription);
            TempData["Subscription"] = passedData;
        }
        return Page();
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
                if (TempData["SubscriptionDev"] != null)
                {
                    var subscriptionViewModel =
                        JsonConvert.DeserializeObject<SubscriptionViewModel>(TempData["SubscriptionDev"].ToString() ??
                                                                             string.Empty);
                    planId = subscriptionViewModel?.planId;
                    logger.LogInformation("Subscription plan id {PlanId}", planId);
                }
            }
            else
            {
                //set the subscription
                if (TempData["Subscription"] != null)
                {
                    var subscription =
                        JsonConvert.DeserializeObject<ResolvedSubscription>(TempData["Subscription"].ToString() ??
                                                                            string.Empty);
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

            if (!string.IsNullOrEmpty(planId))
            {
                var package = await packageRepository.GetPackageBasedOnCodeAsync(planId);
                var selectedPlanId = package.PackageId;
                await packageRepository.SubscribeToPackageAsync(selectedPlanId, user.WebAppUserId);
                logger.LogInformation("User {UserId} subscribed to package {PackageId}", user.WebAppUserId,
                    selectedPlanId);
            }

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