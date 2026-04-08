namespace LegacyRenewalApp.Discounts;

public interface IDiscountCalculator
{
    DiscountCalculationResult Calculate(
        Customer customer,
        SubscriptionPlan plan,
        int seatCount,
        bool useLoyaltyPoints,
        decimal baseAmount);
}