namespace LegacyRenewalApp.Discounts;

public class DiscountCalculationResult
{
    public decimal DiscountAmount { get; set; }
    public decimal SubtotalAfterDiscount { get; set; }
    public string Notes { get; set; } = string.Empty;
}