namespace SaaS.WebApp.ViewModels;

public class SubscriptionViewModel
{
    public string id { get; set; }
    public string name { get; set; }
    public string offerId { get; set; }
    public string planId { get; set; }
    public Beneficiary beneficiary { get; set; }
    public Purchaser purchaser { get; set; }
    public object quantity { get; set; }
    public bool autoRenew { get; set; }
    public bool isTest { get; set; }
    public bool isFreeTrial { get; set; }
}

public class Beneficiary
{
    public string emailId { get; set; }
    public string objectId { get; set; }
    public string tenantId { get; set; }
}

public class Purchaser
{
    public string emailId { get; set; }
    public string objectId { get; set; }
    public string tenantId { get; set; }
}

