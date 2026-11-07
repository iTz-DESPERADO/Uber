using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uber.BLL.Services.Abstraction
{
    public interface IPaymentService
    {
        public string CreateCheckoutSession(string customerId, long amount, string successUrl, string cancelUrl);
        public PaymentIntent ChargeSavedCard(string customerId, string paymentMethodId, long amount);
        public string CreateStripeCustomer(string email, string name);
        public List<PaymentMethod> GetSavedCards(string customerId);
    }
}
