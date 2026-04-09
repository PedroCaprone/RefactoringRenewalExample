namespace LegacyRenewalApp.Pricing;

public interface ITaxCalculator
{
    decimal Calculate(string country, decimal subtotalAfterDiscount, decimal supportFee, decimal paymentFee);
}