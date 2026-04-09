using System;
using LegacyRenewalApp.Billing;
using LegacyRenewalApp.Discounts;
using LegacyRenewalApp.Pricing;
using LegacyRenewalApp.Validation;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly ILegacyBillingService _billingService;
        private readonly IRenewalRequestValidator _validator;
        private readonly IDiscountCalculator _discountCalculator;
        private readonly ISupportFeeCalculator _supportFeeCalculator;
        private readonly IPaymentFeeCalculator _paymentFeeCalculator;
        private readonly ITaxCalculator _taxCalculator;
        
        public SubscriptionRenewalService()
            : this(new CustomerRepository(), 
                new SubscriptionPlanRepository(),
                new LegacyBillingService(),
                new RenewalRequestValidator(),
                new DiscountCalculator(),
                new SupportFeeCalculator(),
                new PaymentFeeCalculator(),
                new TaxCalculator())
        {
        }

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            ILegacyBillingService billingService,
            IRenewalRequestValidator validator,
            IDiscountCalculator discountCalculator,
            ISupportFeeCalculator supportFeeCalculator,
            IPaymentFeeCalculator  paymentFeeCalculator,
            ITaxCalculator  taxCalculator)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billingService = billingService;
            _validator = validator;
            _discountCalculator = discountCalculator;
            _supportFeeCalculator = supportFeeCalculator;
            _paymentFeeCalculator =  paymentFeeCalculator;
            _taxCalculator = taxCalculator;
        }
        
        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            _validator.Validate(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            var discountResult = _discountCalculator.Calculate(
                customer,
                plan,
                seatCount,
                useLoyaltyPoints,
                baseAmount);

            decimal discountAmount = discountResult.DiscountAmount;
            decimal subtotalAfterDiscount = discountResult.SubtotalAfterDiscount;
            string notes = discountResult.Notes;
            
            decimal supportFee = _supportFeeCalculator.Calculate(
                normalizedPlanCode,
                includePremiumSupport);

            if (includePremiumSupport)
            {
                notes += "premium support included; ";
            }

            var paymentFeeResult = _paymentFeeCalculator.Calculate(
                normalizedPaymentMethod,
                subtotalAfterDiscount,
                supportFee);

            decimal paymentFee = paymentFeeResult.PaymentFee;
            notes += paymentFeeResult.Notes;

            decimal taxAmount = _taxCalculator.Calculate(
                customer.Country,
                subtotalAfterDiscount,
                supportFee,
                paymentFee);
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
            
            _billingService.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingService.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
