using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Marketplace.SaaS;
using Newtonsoft.Json;
using SaaS.WebApp.Core;
using SaaS.WebApp.Data;
using SaaS.WebApp.Models;
using SaaS.WebApp.Options;

namespace SaaS.WebApp.Controllers;

/*
 * Webhook event	1. When received	2. If accepted	3. If rejected
 * ChangePlan	Respond with HTTP 200	PATCH with success (this event is optional and autoaccepted in 10 secs)	PATCH with failure OR respond with 4xx (within 10 seconds)
 * ChangeQuantity	Respond with HTTP 200	PATCH with success (this event is optional and autoaccepted in 10 secs)	PATCH with failure OR respond with 4xx (within 10 seconds)
 * Renew	Respond with HTTP 200	Not applicable	Not applicable
 * Suspend	Respond with HTTP 200	Not applicable	Not applicable
 * Unsubscribe	Respond with HTTP 200	Not applicable	Not applicable
 * Reinstate	Respond with HTTP 200	Not applicable	Not applicable (call delete API to trigger deletion if reinstate can't be accepted)
 *
 *
 */
[ApiController, AllowAnonymous, Route(RouteHelper.RouteWebhook)]
public class WebhookController(
    ILogger<WebhookController> logger,
    MarketplaceSaaSClient marketplaceSaaSClient,
    PackageRepository packageRepository,
    WebAppUserRepository webAppUserRepository,
    IWebHostEnvironment webHostEnvironment,
    IOptions<EntraOptions> entraOptionsValue)
    : Controller
{
    [HttpGet]
    [Route(RouteHelper.HealthRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult IsAlive()
    {
        logger.LogInformation("Called alive endpoint at {DateCalled}", DateTime.UtcNow);
        return new ContentResult
            { StatusCode = 200, Content = $"I am alive at {DateTime.Now} on {Environment.MachineName}" };
    }

    [HttpPost]
    [Route(RouteHelper.WebHookRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> WebHookActionsAsync(WebhookPayload request)
    {
        if (webHostEnvironment.IsProduction())
        {
            try
            {
                logger.LogInformation("Validating the JWT token.");
                var token = HttpContext.Request.Headers.Authorization.ToString().Split(' ')[1];
                if (await ValidateTokenAsync(token))
                    logger.LogInformation("Token is validated successfully.");
                else
                {
                    logger.LogError("Token validation failed.");
                    return new UnauthorizedResult();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while validating the token.");
                return new UnauthorizedResult();
            }
        }

        if (request == null) throw new Exception("Request payload is null.");

        logger.LogInformation("Received Webhook request: {WebhookRequest}", JsonConvert.SerializeObject(request));

        var currentPackageId = 0;
        if (!string.IsNullOrEmpty(request.PlanId))
        {
            var package = await packageRepository.GetPackageBasedOnCodeAsync(request.PlanId);
            currentPackageId = package.PackageId;
        }

        var userId = 0;
        if (!string.IsNullOrEmpty(request.PublisherId))
        {
            var user = await webAppUserRepository.GetUserByEmailAsync(request.PublisherId);
            userId = user.WebAppUserId;
        }
        
        switch (request.Action)
        {
            case WebhookAction.Unsubscribe:
            {
                await packageRepository.UnSubscribeFromPackageAsync(currentPackageId,userId);
                break;
            }
            case WebhookAction.ChangePlan:
                break;
            case WebhookAction.ChangeQuantity:
                break;
            case WebhookAction.Suspend:
                break;
            case WebhookAction.Reinstate:
                break;
            case WebhookAction.Renew:
                break;
            case WebhookAction.Transfer:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Ok();
    }

    private async Task<bool> ValidateTokenAsync(string token)
    {
        var entraOptions = entraOptionsValue.Value;
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://login.microsoftonline.com/{entraOptions.TenantId}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());
        var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
        var signingKeys = openIdConfig.SigningKeys;
        var audience = entraOptions.ClientId;

        var validationParameters = new TokenValidationParameters
        {
            //Issuer can change based on the token version. So we are not validating the issuer
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(0)
        };

        var handler = new JwtSecurityTokenHandler();

        //validate aud, expiry and signature using jwt validation handler
        var claimsPrincipal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        // Get the 'tid' claim
        var tidClaim = claimsPrincipal.FindFirst("tid");
        var tenantId = tidClaim?.Value;

        var tidfullClaim = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid");
        var tidfull = tidfullClaim?.Value;

        //For the old token v1, its 'appId' and for v2 its 'azp'. Try get both.
        var azpClaim = claimsPrincipal.FindFirst("azp");
        var azpId = azpClaim?.Value;

        var appidClaim = claimsPrincipal.FindFirst("appid");
        var appId = appidClaim?.Value;

        //return false if the tenantId or azpId or appId is not matching with the configuration
        if (tenantId != entraOptions.TenantId && tidfull != entraOptions.TenantId)
            throw new Exception("TenantId is not matching with the configuration");

        if (azpId != entraOptions.ClientId && appId != entraOptions.ClientId)
            throw new Exception("azpId or appId is not matching with the configuration");

        return true;
    }
}