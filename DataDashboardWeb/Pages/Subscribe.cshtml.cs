using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataDashboardWebLib;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using DataDashboardMessagingLibCore;
using DataDashboardMessagingLibCore.Models;

namespace DataDashboardWeb.Pages
{
    public class SubscribeModel : PageModel
    {
        public string ErrorMessage { get; set; }

        public decimal SubscriptionPrice { get; set; } = 1500M;

        [BindProperty]
        public SubscribeFormModel SubscribeForm { get; set; }

        public string PromoCode { get; set; }

        private readonly AppSettings _appSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDistributedCache _distributedCache;

        public SubscribeModel(IOptions<AppSettings> appSettings, IHostingEnvironment hostingEnvironment, IDistributedCache distributedCache)
        {
            _appSettings = appSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _distributedCache = distributedCache;
        }

        public void OnGet(string promocode)
        {
            // Get any passed in promotional code
            PromoCode = promocode;

            ApplyPromotion();

            // Get the public Stripe key to include in the form
            SubscribeForm = new SubscribeFormModel
            {
                StripeKey = _appSettings.StripePublicKey
            };

            // Oddly, have to assign something to the session in order for it to retain itself 
            // and so be able to get the ID across pages, whatever!
            HttpContext.Session.SetString("something", "something");
        }

        public async Task<IActionResult> OnPostAsync(string promocode)
        {
            // Get any passed in promotional code
            PromoCode = promocode;

            ApplyPromotion();

            // GET REQUIRED DATA ////////////////////////////////////////////////////////////////////////////////////
            // Payment settings
            decimal subscriptionPrice = SubscriptionPrice; // This is in the global variable as we show it to the user on screen
            decimal taxRate = 0.2M;
            decimal paymentAmount = Convert.ToInt32(subscriptionPrice * (taxRate + 1));

            // Need to get some app settings
            string apiUrl = _appSettings.ApiUrl; // Get URL of our API
            string azureStorageConnection = _appSettings.AzureStorageConnection; // Get the connection string for Azure Storage Queues
            string reCaptchaKey = _appSettings.reCaptchaSecretKey; // Get reCAPTCHA secret key
            string stripePrivateKey = _appSettings.StripePrivateKey; // Stripe private key

            // Path to the email template
            string webRootPath = _hostingEnvironment.WebRootPath;
            string emailTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                        + "templates" + Path.DirectorySeparatorChar.ToString()
                                        + "email" + Path.DirectorySeparatorChar.ToString()
                                        + "NewSubscriptionTemplate.html";

            // Get values for use by reCAPTCHA
            string recaptchaResponse = Request.Form["g-recaptcha-response"]; // Hidden field
            string usersIPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(); // User's IP address

            // Get data from our form
            string firstName = SubscribeForm.FirstName;
            string lastName = SubscribeForm.LastName;
            string email = SubscribeForm.Email;

            // Communication Preferences (GDPR compliance)
            bool isCommsOkay = (bool)SubscribeForm.IsCommsOkay;

            // Get the payment token submitted by the form:
            string stripeToken = Request.Form["stripeSource"];
            string stripeThreeDSecure = Request.Form["three_d_secure"];

            // We need this in order to track our payment through any multi-step process such as 3D Secure
            // We default it to the session Id in the event that there is no Stripe ClientSecret
            string cacheKey = HttpContext.Session.Id;

            // Keep a running transaction, the result of most steps get assigned here
            TransactionResult result = new TransactionResult()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                StripeToken = stripeToken,
                SubscriptionPrice = Convert.ToInt32(subscriptionPrice),
                TaxRate = taxRate,
                IsCommsOkay = isCommsOkay
            };

            // Now begin the process of validation, payment, user and subscription creation...
            // This is our transaction helper
            TransactionAccess transactionAccess = new TransactionAccess(apiUrl);

            // BEGIN PROCESSING ////////////////////////////////////////////////////////////////////////////////////
            // (1) Validate the reCAPTCHA widget, if fails then show message and stop
            ReCaptchaResult recaptchaResult = await transactionAccess.ValidateReCaptcha(reCaptchaKey, recaptchaResponse, usersIPAddress);
            if (recaptchaResult.IsSuccess == false)
            {
                ErrorMessage = recaptchaResult.ErrorMessage;
                return Page();
            }

            // (2) Now create a new user, if fails then show message and stop
            result.User = await transactionAccess.CreateNewUser(email, firstName, lastName, "subscriber");
            if (result.User.IsSuccess == false)
            {
                ErrorMessage = result.User.ErrorMessage;
                return Page();
            }

            // (3) Make the payment, if fails then remove user, show message and stop
            PaymentAccess paymentAccess = new PaymentAccess(stripePrivateKey);

            // Is this a 3D Secure payment?
            if (stripeThreeDSecure == "required")
            {
                // 3D Secure and so we're potentially going to enter a multi-step process
                string returnUrl = string.Format("https://{0}/ThankYou", HttpContext.Request.Host.Value);
                result.Payment = paymentAccess.Make3DSecurePayment(paymentAmount, stripeToken, returnUrl);

                if (result.Payment.IsError)
                {
                    // If payment fails then remove the user that we created
                    // The user may want to try again and having them exist would block that
                    UserAccess userAccess = new UserAccess(apiUrl);
                    await userAccess.Delete(result.User.UserId);

                    ErrorMessage = result.Payment.ErrorMessage;

                    return Page();
                }
                else if (result.Payment.Status == "pending") // If it's Pending then we redirect but if it's Chargeable then we can just charge now
                {
                    cacheKey = result.Payment.ClientSecret;

                    await CacheTransactionResult(cacheKey, result);

                    return Redirect(result.Payment.RedirectUrl);
                }
            }

            // Got this far then it's a regular payment and so we keep on going
            result.Payment = paymentAccess.MakePayment(paymentAmount, stripeToken, "Discovered Insights Subscription");

            if (result.Payment.IsSuccess == false)
            {
                // If payment fails then remove the user that we created
                // The user may want to try again and having them exist would block that
                UserAccess userAccess = new UserAccess(apiUrl);
                await userAccess.Delete(result.User.UserId);

                ErrorMessage = result.Payment.ErrorMessage;
                return Page();
            }

            // (4) Create the subscription, if fails then 
            result.Subscription = await transactionAccess.CreateNewSubscription(result.User.UserId, result.Payment.PaymentReference, (decimal)result.Payment.AmountPaid, true, result.IsCommsOkay, 1);
            if (result.Subscription.IsSuccess == false)
            {
                ErrorMessage = result.Subscription.ErrorMessage;
                return Page();
            }

            // Email time
            EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

            // (5) Create the email
            int taxAmount = Convert.ToInt32(result.SubscriptionPrice * result.TaxRate);
            string body = emailAccess.CreateMessageBody(emailTemplatePath, firstName, result.User.Password, result.Payment.AmountPaid.ToString(), 
                                                            result.Subscription.SubscriptionReference, result.SubscriptionPrice.ToString(), 
                                                            (Convert.ToInt32(result.TaxRate*100)).ToString(), taxAmount.ToString());

            // (6) Create the email object
            OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail(email, "no-reply@discoveredinsights.com", 
                                                                            "Welcome to Discovered Insights - " + result.Subscription.SubscriptionReference, body);

            // (7) Send the email
            await emailAccess.Send(outgoingEmail);

            // If we've got this far then the transaction is complete
            result.IsComplete = true;

            // FINISH UP ////////////////////////////////////////////////////////////////////////////////////
            await CacheTransactionResult(cacheKey, result);

            return Redirect("ThankYou");
        }

        private async Task CacheTransactionResult(string cacheKey, TransactionResult result)
        {
            // We cache the result data and move to the next step (outside of the site)
            // Serialize the object for storage
            var options = new DistributedCacheEntryOptions();
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Keep this data for 10 minutes
            await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(result), options);
        }

        private void ApplyPromotion()
        {
            // Handle promotion code discounts that are passed via the URL
            if (string.IsNullOrWhiteSpace(PromoCode) == false)
            {
                // NOT HOW THIS SHOULD BE DONE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // THIS SHOULD BE DRIVEN VIA THE API !!!!!!!!!!!!!!!!!!!!!!!!!!
                if (PromoCode == "dennispub")
                {
                    SubscriptionPrice = 1000M;
                }
            }
        }
    }

    public class SubscribeFormModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Terms and Conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please accept the terms and conditions")]
        public bool TermsAndConditions { get; set; }

        [Required(ErrorMessage = "Please select a communication preference")]
        public bool? IsCommsOkay { get; set; } = null;

        [Required]
        [Display(Name = "reCAPTCHA")]
        public string ReCaptcha { get; set; }
        public string StripeKey { get; set; }
    }
}