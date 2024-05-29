namespace SaaS.WebApp.Models;

public class Package
{
    public int PackageId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float Price { get; set; } = 0.0f;
    public int UserCount { get; set; } = 0;
}