namespace LegacyRenewalApp.Billing;

public interface ILegacyBillingService
{
    void SaveInvoice(RenewalInvoice invoice);
    void SendEmail(string email, string subject, string body);
}