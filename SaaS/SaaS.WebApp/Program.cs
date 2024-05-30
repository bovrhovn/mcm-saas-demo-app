using System.Net;
using System.Text.Json.Serialization;
using Azure.Identity;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Marketplace.SaaS;
using SaaS.WebApp.Core;
using SaaS.WebApp.Data;
using SaaS.WebApp.Options;

var builder = WebApplication.CreateBuilder(args);
//options
builder.Services.AddOptions<SqlOptions>()
    .Bind(builder.Configuration.GetSection("SqlOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<EntraOptions>()
    .Bind(builder.Configuration.GetSection("SaaSOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

//services configuration
builder.Services.AddSingleton<UserDataContext>();
builder.Services.AddSingleton<WebAppUserRepository>();
builder.Services.AddSingleton<PackageRepository>();
var entraOptions = builder.Configuration.GetSection("SaaSOptions").Get<EntraOptions>();
builder.Services.AddScoped<IMarketplaceSaaSClient, MarketplaceSaaSClient>(_ =>
    new MarketplaceSaaSClient(new ClientSecretCredential(entraOptions.TenantId, entraOptions.ClientId,
        entraOptions.Secret)));

//system settings
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages().AddRazorPagesOptions(options => options.Conventions.AddPageRoute("/Info/Index", ""));
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = new PathString("/User/Login"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(new CorsPolicyBuilder()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin()
        .Build());
});
var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Info/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>();
        if (exception != null)
        {
            var message = $"{exception.Error.Message}";
            await context.Response.WriteAsync(message).ConfigureAwait(false);
        }
    });
});
app.MapHealthChecks($"/{RouteHelper.HealthRoute}", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

app.Run();