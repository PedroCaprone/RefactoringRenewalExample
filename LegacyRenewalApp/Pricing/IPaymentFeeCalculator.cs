namespace LegacyRenewalApp.Pricing;

public interface IPaymentFeeCalculator
{
    PaymentFeeCalculationResult Calculate(
        string paymentMethod,
        decimal subtotalAfterDiscount,
        decimal supportFee);
}