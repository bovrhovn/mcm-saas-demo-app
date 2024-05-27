using System.ComponentModel.DataAnnotations;

namespace SaaS.WebApp.Options;

public class SqlOptions
{
    [Required(ErrorMessage = "Connection string to SQL server is required")]
    public string ConnectionString { get; set; }
}