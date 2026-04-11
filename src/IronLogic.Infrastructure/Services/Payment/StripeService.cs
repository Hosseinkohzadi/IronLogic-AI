using IronLogic.Application.Interfaces;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;
using IronLogic.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace IronLogic.Infrastructure.Services.Payment;

/// <summary>
/// Stripe payment gateway integration service.
/// Handles multi-currency subscriptions (CAD, USD, EUR) and Canadian tax compliance (GST/HST).
/// </summary>
/// <param name="settings">Stripe configuration settings.</param>
/// <param name="subscriptionService">Subscription business logic service.</param>
/// <param name="planRepository">Subscription plan repository.</param>
/// <param name="logger">Logger for tracking payment operations.</param>
public class StripeService(
    IOptions<StripeSettings> settings,
    ISubscriptionService subscriptionService,
    IGenericRepository<Domain.Entities.SubscriptionPlan> planRepository,
    ILogger<StripeService> logger) : IStripeService
{
    private readonly StripeSettings _settings = settings.Value;

    /// <inheritdoc />
    public async Task<string> CreateCheckoutSessionAsync(
        Guid planId,
        string userEmail,
        string userId,
        Currency currency,
        string countryCode,
        string? regionCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        StripeConfiguration.ApiKey = _settings.SecretKey;

        var plan = await planRepository.GetByIdAsync(planId);
        if (plan == null)
            throw new InvalidOperationException($"Subscription plan with ID {planId} not found.");

        var taxAmount = CalculateTaxAmount(plan.Price, countryCode, regionCode);
        var totalAmount = plan.Price + taxAmount;

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency.ToString().ToLowerInvariant(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{plan.Name} Plan",
                            Description = plan.FeaturesJson ?? "IronLogic AI Subscription"
                        },
                        UnitAmount = (long)(plan.Price * 100),
                        Recurring = new SessionLineItemPriceDataRecurringOptions
                        {
                            Interval = plan.DurationDays switch
                            {
                                30 => "month",
                                365 => "year",
                                _ => "month"
                            }
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "subscription",
            SuccessUrl = _settings.SuccessUrl,
            CancelUrl = _settings.CancelUrl,
            CustomerEmail = userEmail,
            ClientReferenceId = userId,
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId },
                { "planId", planId.ToString() },
                { "countryCode", countryCode },
                { "regionCode", regionCode ?? string.Empty },
                { "taxAmount", taxAmount.ToString("F2") },
                { "currency", currency.ToString() }
            }
        };

        if (_settings.UseStripeTax)
        {
            options.AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true };
        }

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        logger.LogInformation(
            "Stripe Checkout Session created: {SessionId} for user {UserId}, plan {PlanId}, currency {Currency}",
            session.Id, userId, planId, currency);

        return session.Id;
    }

    /// <inheritdoc />
    public async Task<bool> HandleWebhookAsync(string json, string stripeSignature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSignature);

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _settings.WebhookSecret);

            logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            return stripeEvent.Type switch
            {
                Events.CheckoutSessionCompleted => await HandleCheckoutSessionCompletedAsync(stripeEvent),
                Events.InvoicePaid => await HandleInvoicePaidAsync(stripeEvent),
                Events.CustomerSubscriptionDeleted => await HandleSubscriptionDeletedAsync(stripeEvent),
                Events.CustomerSubscriptionUpdated => await HandleSubscriptionUpdatedAsync(stripeEvent),
                _ => true
            };
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook signature verification failed");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetOrCreateCustomerAsync(string userId, string userEmail)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        var options = new CustomerSearchOptions
        {
            Query = $"email:'{userEmail}'"
        };

        var service = new CustomerService();
        var customers = await service.SearchAsync(options);

        if (customers.Data.Count > 0)
            return customers.Data[0].Id;

        var createOptions = new CustomerCreateOptions
        {
            Email = userEmail,
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId }
            }
        };

        var customer = await service.CreateAsync(createOptions);
        return customer.Id;
    }

    /// <inheritdoc />
    public async Task<bool> CancelSubscriptionAsync(string stripeSubscriptionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSubscriptionId);

        try
        {
            StripeConfiguration.ApiKey = _settings.SecretKey;

            var service = new SubscriptionService();
            await service.CancelAsync(stripeSubscriptionId);

            logger.LogInformation("Stripe subscription cancelled: {SubscriptionId}", stripeSubscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel Stripe subscription: {SubscriptionId}", stripeSubscriptionId);
            return false;
        }
    }

    /// <inheritdoc />
    public decimal CalculateTaxAmount(decimal subtotal, string countryCode, string? regionCode)
    {
        if (countryCode.Equals("CA", StringComparison.OrdinalIgnoreCase))
        {
            var taxRate = GetCanadianTaxRate(regionCode);
            return Math.Round(subtotal * taxRate, 2);
        }

        return 0m;
    }

    /// <summary>
    /// Handles the checkout.session.completed webhook event.
    /// Activates the subscription after successful payment.
    /// </summary>
    private async Task<bool> HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null)
            return false;

        var userId = session.ClientReferenceId ?? session.Metadata?["userId"];
        var planIdStr = session.Metadata?["planId"];
        var countryCode = session.Metadata?["countryCode"] ?? "US";
        var regionCode = session.Metadata?["regionCode"];
        var taxAmountStr = session.Metadata?["taxAmount"];
        var currencyStr = session.Metadata?["currency"] ?? "USD";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(planIdStr))
            return false;

        var planId = Guid.Parse(planIdStr);
        var currency = Enum.Parse<Currency>(currencyStr, true);
        var taxAmount = decimal.TryParse(taxAmountStr, out var tax) ? tax : 0m;
        var amount = session.AmountTotal.HasValue ? session.AmountTotal.Value / 100m : 0m;

        await subscriptionService.ActivateSubscriptionAsync(
            userId,
            planId,
            session.SubscriptionId,
            session.CustomerId,
            amount,
            taxAmount,
            currency,
            countryCode,
            regionCode);

        logger.LogInformation("Subscription activated for user {UserId} via checkout session {SessionId}", userId, session.Id);
        return true;
    }

    /// <summary>
    /// Handles the invoice.paid webhook event.
    /// Records the payment transaction for subscription renewals.
    /// </summary>
    private async Task<bool> HandleInvoicePaidAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null)
            return false;

        var userId = invoice.Metadata?.GetValueOrDefault("userId");
        if (string.IsNullOrEmpty(userId))
            return false;

        var amount = invoice.AmountPaid / 100m;
        var taxAmount = invoice.Tax.HasValue ? invoice.Tax.Value / 100m : 0m;
        var currency = Enum.Parse<Currency>(invoice.Currency.ToUpperInvariant(), true);

        await subscriptionService.RecordPaymentAsync(
            userId,
            amount,
            taxAmount,
            currency,
            invoice.PaymentIntentId,
            invoice.SubscriptionId,
            invoice.Id);

        logger.LogInformation("Payment recorded for user {UserId}, invoice {InvoiceId}", userId, invoice.Id);
        return true;
    }

    /// <summary>
    /// Handles the customer.subscription.deleted webhook event.
    /// Deactivates the subscription in the system.
    /// </summary>
    private async Task<bool> HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null)
            return false;

        await subscriptionService.DeactivateSubscriptionAsync(subscription.Id, "Subscription cancelled by customer");

        logger.LogInformation("Subscription deactivated: {SubscriptionId}", subscription.Id);
        return true;
    }

    /// <summary>
    /// Handles the customer.subscription.updated webhook event.
    /// Updates subscription status if needed.
    /// </summary>
    private async Task<bool> HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null)
            return false;

        if (subscription.Status == "canceled" || subscription.Status == "unpaid")
        {
            await subscriptionService.DeactivateSubscriptionAsync(subscription.Id, $"Subscription status: {subscription.Status}");
            logger.LogInformation("Subscription deactivated due to status change: {SubscriptionId}, status: {Status}", subscription.Id, subscription.Status);
        }

        return true;
    }

    /// <summary>
    /// Gets the tax rate for Canadian provinces (GST/HST/PST).
    /// </summary>
    /// <param name="regionCode">The province code (e.g., "ON", "BC", "AB").</param>
    /// <returns>The decimal tax rate (e.g., 0.13 for 13% HST in Ontario).</returns>
    private decimal GetCanadianTaxRate(string? regionCode)
    {
        return regionCode?.ToUpperInvariant() switch
        {
            "ON" => 0.13m,    // Ontario HST 13%
            "BC" => 0.12m,    // BC GST 5% + PST 7% = 12%
            "AB" => 0.05m,    // Alberta GST 5%
            "QC" => 0.14975m, // Quebec GST 5% + QST 9.975% = 14.975%
            "NB" => 0.15m,    // New Brunswick HST 15%
            "NS" => 0.15m,    // Nova Scotia HST 15%
            "PE" => 0.15m,    // Prince Edward Island HST 15%
            "NL" => 0.15m,    // Newfoundland and Labrador HST 15%
            "MB" => 0.12m,    // Manitoba GST 5% + PST 7% = 12%
            "SK" => 0.11m,    // Saskatchewan GST 5% + PST 6% = 11%
            "YT" => 0.05m,    // Yukon GST 5%
            "NT" => 0.05m,    // Northwest Territories GST 5%
            "NU" => 0.05m,    // Nunavut GST 5%
            _ => _settings.DefaultCanadianTaxRate
        };
    }
}
