namespace LegacyRenewalApp.Pricing;

public interface ISupportFeeCalculator
{
    decimal Calculate(string planCode, bool includePremiumSupport);
}