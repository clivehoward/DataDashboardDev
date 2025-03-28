using DataDashboardMessagingLibCore;
using DataDashboardMessagingLibCore.Models;
using DataDashboardWebLib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DataDashboardWeb.Pages
{
    [Produces("application/json")]
    [Route("webhooks/Payment")]
    public class PaymentController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly IDistributedCache _distributedCache;
        private readonly IHostingEnvironment _hostingEnvironment;

        public PaymentController(IOptions<AppSettings> appSettings, IHostingEnvironment hostingEnvironment, IDistributedCache distributedCache)
        {
            _appSettings = appSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _distributedCache = distributedCache;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Need to get some app settings
                string apiUrl = _appSettings.ApiUrl; // Get URL of our API
                string azureStorageConnection = _appSettings.AzureStorageConnection; // Get the connection string for Azure Storage Queues
                string stripePrivateKey = _appSettings.StripePrivateKey; // Stripe private key

                string webRootPath = _hostingEnvironment.WebRootPath; // Path to application root
                string secret = _appSettings.StripeWebhookSecret; // This secret is specific to the webhook
                string signature = Request.Headers["Stripe-Signature"]; // Get data that has been passed
                string json = new StreamReader(HttpContext.Request.Body).ReadToEnd(); // Get the response from Stripe

                string emailTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                                        + "templates" + Path.DirectorySeparatorChar.ToString()
                                                        + "email" + Path.DirectorySeparatorChar.ToString();

                // Now we have all we need to validate the data against Stripe
                PaymentAccess paymentAccess = new PaymentAccess(stripePrivateKey);
                PaymentPostResult postResult = paymentAccess.GetPaymentPostResult(json, signature, secret);

                // Failed then give up here.
                if (postResult.IsSuccess == false)
                {
                    return Ok();
                }

                // To identify this payment we need the ClientSecret
                // This is created at the beginning and then passed throughout including to our return URL in the browser
                // Need to make sure that we have this...
                string clientSecret = postResult.ClientSecret;

                // Transaction was cached as serialized JSON
                var cachedObject = await _distributedCache.GetStringAsync(clientSecret);

                // If nothing in the cache then give up here
                if (cachedObject == null)
                {
                    return Ok();
                }

                TransactionResult transaction = JsonConvert.DeserializeObject<TransactionResult>(cachedObject);

                // Get some data we need from the transaction
                string sourceId = transaction.Payment.SourceId; // Get the source ID
                int paymentAmount = transaction.Payment.AmountPaid; // Amount to be paid

                // Now begin the process of validation, payment, user and subscription creation...
                // This is our transaction helper
                TransactionAccess transactionAccess = new TransactionAccess(apiUrl);
                UserAccess userAccess = new UserAccess(apiUrl);

                // What type of event has occurred?
                switch (postResult.EventType)
                {
                    case "source.chargeable": // We can complete the payment and process

                        transaction.Payment = paymentAccess.MakePayment(paymentAmount, sourceId, "Discovered Insights Subscription");

                        // If faile then handle it here
                        if (transaction.Payment.IsSuccess == false)
                        {
                            await TransactionFailed(azureStorageConnection, emailTemplatePath, clientSecret, transaction, transactionAccess, userAccess);
                            break;
                        }

                        // (4) Create the subscription, if fails then 
                        transaction.Subscription = await transactionAccess.CreateNewSubscription(transaction.User.UserId, transaction.Payment.PaymentReference, (decimal)transaction.Payment.AmountPaid, true, transaction.IsCommsOkay, 1);

                        // If that failed then give up here.
                        if (transaction.Subscription.IsSuccess == false)
                        {
                            break;
                        }

                        // Email time
                        EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

                        // Path to the email template
                        emailTemplatePath = emailTemplatePath + "NewSubscriptionTemplate.html";

                        // (5) Create the email
                        int taxAmount = Convert.ToInt32(transaction.SubscriptionPrice * transaction.TaxRate);
                        string body = emailAccess.CreateMessageBody(emailTemplatePath, transaction.FirstName, transaction.User.Password,
                                                                        transaction.Payment.AmountPaid.ToString(),
                                                                        transaction.Subscription.SubscriptionReference,
                                                                        transaction.SubscriptionPrice.ToString(),
                                                                        (Convert.ToInt32(transaction.TaxRate * 100)).ToString(), 
                                                                        taxAmount.ToString());

                        // (6) Create the email object
                        OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail(transaction.Email, "no-reply@discoveredinsights.com",
                                                                                        "Welcome to Discovered Insights - " + transaction.Subscription.SubscriptionReference, body);

                        // (7) Send the email
                        await emailAccess.Send(outgoingEmail);

                        // If we've got this far then the transaction is complete
                        transaction.IsComplete = true;

                        // FINISH UP ////////////////////////////////////////////////////////////////////////////////////
                        await CacheTransactionResult(clientSecret, transaction);

                        break;
                    case "source.failed": // 3D Secure failed
                        transaction.Payment.IsError = true;
                        transaction.Payment.ErrorMessage = "Sorry, your 3D Secure verification failed.";
                        await TransactionFailed(azureStorageConnection, emailTemplatePath, clientSecret, transaction, transactionAccess, userAccess);
                        break;
                    case "source.canceled": // User cancelled out of the 3D Secure process
                        transaction.Payment.IsError = true;
                        transaction.Payment.ErrorMessage = "Sorry, your payment was cancelled.";
                        await TransactionFailed(azureStorageConnection, emailTemplatePath, clientSecret, transaction, transactionAccess, userAccess);
                        break;
                }
            }
            catch(Exception ex)
            {
                // Should probably do something here !!!
            }

            return Ok(); // No matter what we return okay
        }

        private async Task TransactionFailed(string azureStorageConnection, string emailTemplatePath, string clientSecret, 
                                                TransactionResult transaction, TransactionAccess transactionAccess, UserAccess userAccess)
        {
            // If payment fails then remove the user that we created
            // The user may want to try again and having them exist would block that
            await userAccess.Delete(transaction.User.UserId);

            // Need to notify the user that this transaction failed
            // Email time
            EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

            // Path to the email template
            emailTemplatePath = emailTemplatePath + "NewSubscriptionFailedTemplate.html";

            // (5) Create the email
            string body = emailAccess.CreateMessageBody(emailTemplatePath, transaction.FirstName);

            // (6) Create the email object
            OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail(transaction.Email, "no-reply@discoveredinsights.com",
                                                                            "Discovered Insights Subscription Failed", body);

            // (7) Send the email
            await emailAccess.Send(outgoingEmail);

            // If we've got this far then the transaction is complete (even though it failed)
            transaction.IsError = true;
            transaction.ErrorMessage = transaction.Payment.ErrorMessage;
            transaction.IsComplete = true;

            // FINISH UP ////////////////////////////////////////////////////////////////////////////////////
            await CacheTransactionResult(clientSecret, transaction);

            //System.IO.File.WriteAllText(@"C:\Users\Clive Howard\Documents\Result.txt", transaction.Payment.ErrorMessage);
        }

        private async Task CacheTransactionResult(string cacheKey, TransactionResult result)
        {
            // We cache the result data and move to the next step (outside of the site)
            // Serialize the object for storage
            var options = new DistributedCacheEntryOptions();
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Keep this data for 10 minutes
            await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(result), options);
        }
    }
}