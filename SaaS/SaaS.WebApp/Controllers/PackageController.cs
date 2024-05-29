using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.WebApp.Core;
using SaaS.WebApp.Data;
using SaaS.WebApp.ViewModels;

namespace SaaS.WebApp.Controllers;

[AllowAnonymous, ApiController, Route(RouteHelper.RoutePackages),
 Produces(MediaTypeNames.Application.Json)]
public class PackageController(ILogger<PackageController> logger, PackageRepository packageRepository) : Controller
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
    [Route(RouteHelper.SubscribeRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubscribeToPackageAsync([FromBody] SubscriptionPackageViewModel package)
    {
        logger.LogInformation("Subscribing to package {PackageId} with user {UserId}", package.PackageId, package.UserId);
        try
        {
            var result = await packageRepository.SubscribeToPackageAsync(package.PackageId, package.UserId);
            logger.LogInformation("User {UserId} subscribed from package {PackageId}. Result: {Result}", package.UserId,
                package.PackageId, result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest("An error occurred while subscribing to the package. Please try again later.");
        }
        return Ok();
    }
    
    [HttpPost]
    [Route(RouteHelper.UnsubscribeRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnSubscribeToPackageAsync([FromBody] SubscriptionPackageViewModel package)
    {
        logger.LogInformation("Unsubscribing to package {PackageId} with user {UserId}", package.PackageId, package.UserId);
        try
        {
            var result = await packageRepository.UnSubscribeFromPackageAsync(package.PackageId, package.UserId);
            logger.LogInformation("User {UserId} unsubscribed from package {PackageId}. Result: {Result}", package.UserId,
                package.PackageId, result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest("An error occurred while unsubscribing to the package. Please try again later.");
        }
        return Ok();
    }
}