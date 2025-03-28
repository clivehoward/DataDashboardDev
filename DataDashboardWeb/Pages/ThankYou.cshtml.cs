using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using DataDashboardWebLib;

namespace DataDashboardWeb.Pages
{
    public class ThankYouModel : PageModel
    {
        public string SubscriptionReference { get; set; }
        public string Output { get; set; }

        public bool IsCompleteSuccess { get; set; } = false;
        public bool IsCompleteFail { get; set; } = false;
        public bool IsPending { get; set; } = false;

        private readonly IDistributedCache _distributedCache;

        public ThankYouModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<IActionResult> OnGetAsync(string client_secret)
        {
            // Get the payment transaction from the cache
            // If there is a ClientSecret then we use that, otherwise...
            // Use the session ID as our cache key
            string cacheKey = HttpContext.Session.Id;

            if (string.IsNullOrWhiteSpace(client_secret) == false)
            {
                cacheKey = client_secret;
            }

            // Payment Transaction was cached as serialized JSON
            var cachedObject = await _distributedCache.GetStringAsync(cacheKey);

            Output = cachedObject; // Outputting the JSON for testing purposes

            // If no data then just send them to homepage
            if(cachedObject != null)
            {
                TransactionResult transaction = JsonConvert.DeserializeObject<TransactionResult>(cachedObject);

                // What do we know about this transaction
                if (transaction.IsComplete == true && transaction.IsError == false)
                {
                    IsCompleteSuccess = true; // Completed without errors so we must be all good  
                    SubscriptionReference = transaction.Subscription.SubscriptionReference;
                }

                if (transaction.IsComplete == true && transaction.IsError == true)
                {
                    IsCompleteFail = true; // Completed but with errors, no reference
                }

                if (transaction.IsComplete == false && transaction.IsError == false)
                {
                    IsPending = true; // Status unknown so use the Stripe source without prefix
                    SubscriptionReference = transaction.Payment.SourceId.Substring(4);
                }
            }
            else
            {
                return Redirect("/Index");
            }

            return Page();
        }
    }
}