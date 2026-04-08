using System;
namespace LegacyRenewalApp.Pricing;

public class PaymentFeeCalculator : IPaymentFeeCalculator
{
    public PaymentFeeCalculationResult Calculate(
        string paymentMethod,
        decimal subtotalAfterDiscount,
        decimal supportFee)
    {
        decimal amountBase = subtotalAfterDiscount + supportFee;

        if (paymentMethod == "CARD")
        {
            return new PaymentFeeCalculationResult
            {
                PaymentFee = amountBase * 0.02m,
                Notes = "card payment fee; "
            };
        }
        else if (paymentMethod == "BANK_TRANSFER")
        {
            return new PaymentFeeCalculationResult
            {
                PaymentFee = amountBase * 0.01m,
                Notes = "bank transfer fee; "
            };
        }
        else if (paymentMethod == "PAYPAL")
        {
            return new PaymentFeeCalculationResult
            {
                PaymentFee = amountBase * 0.035m,
                Notes = "paypal fee; "
            };
        }
        else if (paymentMethod == "INVOICE")
        {
            return new PaymentFeeCalculationResult
            {
                PaymentFee = 0m,
                Notes = "invoice payment; "
            };
        }

        throw new ArgumentException("Unsupported payment method");
    }
}