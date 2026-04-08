namespace LegacyRenewalApp.Pricing;

public class SupportFeeCalculator : ISupportFeeCalculator
{
    public decimal Calculate(string planCode, bool includePremiumSupport)
    {
        if (!includePremiumSupport)
        {
            return 0m;
        }

        if (planCode == "START")
        {
            return 250m;
        }
        else if (planCode == "PRO")
        {
            return 400m;
        }
        else if (planCode == "ENTERPRISE")
        {
            return 700m;
        }

        return 0m;
    }
}