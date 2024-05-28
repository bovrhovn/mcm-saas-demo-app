using System.ComponentModel.DataAnnotations;

namespace SaaS.WebApp.Options;

public class EntraOptions
{
    [Required(ErrorMessage = "Secret is required to call SaaS api's.")]
    public string Secret { get; set; }
    [Required(ErrorMessage = "ClientId is required to call SaaS api's..")]
    public string ClientId { get; set; }
    [Required(ErrorMessage = "TenantId is required to call SaaS api's.")]
    public string TenantId { get; set; }
}