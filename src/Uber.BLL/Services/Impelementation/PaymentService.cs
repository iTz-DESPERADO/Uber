using Uber.BLL.Services.Abstraction;
using Stripe;
using Stripe.Checkout;

namespace Uber.BLL.Services.Impelementation
{
    public class PaymentService : IPaymentService
    {

        public PaymentService(string stripeApiKey)
        {
            StripeConfiguration.ApiKey = stripeApiKey;
        }

        public string CreateCheckoutSession(string customerId, long amount, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                Customer = customerId,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            UnitAmount = amount,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Test Product"
                            }
                        },
                        Quantity = 1
                    }
                },
                SetupIntentData = new SessionSetupIntentDataOptions(), 
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = service.Create(options);

            return session.Url;
        }

        public PaymentIntent ChargeSavedCard(string customerId, string paymentMethodId, long amount)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "egp",
                Customer = customerId,
                PaymentMethod = paymentMethodId,
                OffSession = true,
                Confirm = true
            };

            var service = new PaymentIntentService();
            return service.Create(options);
        }

        public string CreateStripeCustomer(string email, string name)
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name
            };
            var service = new CustomerService();
            var customer = service.Create(options);
            return customer.Id;
        }
        public List<PaymentMethod> GetSavedCards(string customerId)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card"
            };
            var service = new PaymentMethodService();
            return service.List(options).ToList();
        }
    }
}