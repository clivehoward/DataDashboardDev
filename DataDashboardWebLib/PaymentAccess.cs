using DataDashboardWebLib.Interfaces;
using Stripe;
using System;

namespace DataDashboardWebLib
{
    public class PaymentAccess : IPaymentAccess
    {
        private string StripePrivateKey;

        public PaymentAccess(string stripePrivateKey)
        {
            StripePrivateKey = stripePrivateKey;
        }

        public PaymentResult Make3DSecurePayment(decimal price, string stripeToken, string returnUrl)
        {
            PaymentResult result = new PaymentResult
            {
                IsSuccess = false, // Defaults to failure
                SourceId = stripeToken
            };

            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.SetApiKey(StripePrivateKey);

            try
            {
                // For 3D Secure charges
                var sources = new StripeSourceService();
                var source = sources.Create(new StripeSourceCreateOptions
                {
                    Amount = Convert.ToInt32(price * 100), // Seems that this is pence not pounds
                    Currency = "gbp",
                    Type = StripeSourceType.ThreeDSecure,
                    ThreeDSecureCardOrSourceId = stripeToken,
                    RedirectReturnUrl = returnUrl
                });

                result.Status = source.Status;
                result.RedirectUrl = source.Redirect.Url;
                result.ClientSecret = source.ClientSecret;
                result.AmountPaid = (int)price;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorMessage = "Sorry, payment failed due to '" + ex.Message + "'";
            }

            return result;
        }

        public PaymentResult MakePayment(decimal price, string stripeToken, string productDescription)
        {
            PaymentResult result = new PaymentResult
            {
                IsSuccess = false, // Defaults to failure
                SourceId = stripeToken
            };

            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.SetApiKey(StripePrivateKey);

            try
            {
                // Charge the user's card:
                var charges = new StripeChargeService();
                var charge = charges.Create(new StripeChargeCreateOptions
                {
                    Amount = Convert.ToInt32(price * 100), // Seems that this is pence not pounds
                    Currency = "gbp",
                    Description = productDescription,
                    SourceTokenOrExistingSourceId = stripeToken
                });

                if (charge.Status.ToLower() == "succeeded")
                {
                    // Get the charge id which is our payment reference
                    result.Status = charge.Status;
                    result.PaymentReference = charge.Id;
                    result.IsSuccess = true;
                    result.AmountPaid = (int)price;
                }
                else
                {
                    result.IsError = true;
                    result.ErrorMessage = "Sorry, payment failed due to '" + charge.FailureMessage + "'";
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorMessage = "Sorry, payment failed due to '" + ex.Message + "'";
            }

            return result;
        }

        public PaymentPostResult GetPaymentPostResult(string json, string signature, string secret)
        {
            PaymentPostResult result = new PaymentPostResult();

            // Use Stripe to find out what's been passed back (this is data sent to our webhook)
            StripeConfiguration.SetApiKey(StripePrivateKey);

            var stripeEvent = StripeEventUtility.ConstructEvent(json, signature, secret);

            if (stripeEvent != null)
            {
                // Get the source that has been passed from JSON
                StripeSource source = Mapper<StripeSource>.MapFromJson(stripeEvent.Data.Object.ToString());

                // Assign to our result object
                result.ClientSecret = source.ClientSecret;
                result.EventType = stripeEvent.Type;
                result.IsSuccess = true;
            }

            return result;
        }
    }
}
