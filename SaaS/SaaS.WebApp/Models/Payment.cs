namespace SaaS.WebApp.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public string Location { get; set; }
    public string Currency { get; set; }
    public WebAppUser User { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Package> Packages { get; set; }
}