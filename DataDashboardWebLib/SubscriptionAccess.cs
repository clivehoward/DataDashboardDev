using DataDashboardWebLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataDashboardWebLib
{
    public class SubscriptionAccess : ISubscriptionAccess
    {
        private string ApiBaseUri;
        private string BearerToken;

        public SubscriptionAccess(string apiBaseUri)
        {
            ApiBaseUri = apiBaseUri;
        }

        public SubscriptionAccess(string apiBaseUri, string bearerToken)
        {
            ApiBaseUri = apiBaseUri;
            BearerToken = bearerToken;
        }

        public async Task<int> CreateSubscription(string userId, string paymentReference, decimal amountPaid, 
                                                    string subscriptionReference, bool isGDPROptIn, bool isCommsOkay, int enquiryAllocation)
        {
            // Create a new subscription
            int subscriptionId = 0;

            HttpClient client = new HttpClient();

            // Pass in the values
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("UserId", userId),
                new KeyValuePair<string, string>("PaymentReference", paymentReference),
                new KeyValuePair<string, string>("SubscriptionReference", subscriptionReference),
                new KeyValuePair<string, string>("AmountPaid", amountPaid.ToString()),
                new KeyValuePair<string, string>("IsGDPROptIn", isGDPROptIn.ToString()),
                new KeyValuePair<string, string>("IsCommsOkay", isCommsOkay.ToString()),
                new KeyValuePair<string, string>("EnquiryAllocation", enquiryAllocation.ToString()),
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "subscription"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API and check is we're successful
            var result = await client.SendAsync(requestMsg);

            // If the result was successful then we want to get the new subscription identifier
            if (result.IsSuccessStatusCode)
            {
                subscriptionId = await result.Content.ReadAsAsync<int>(); // Get the new subscription ID
            }
            else
            {
                throw new Exception("Subscription failed");
            }

            return subscriptionId;
        }

        public async Task<bool> SetSubscriptionIsActive(int subscriptionId, bool isActive)
        {
            // Change the active setting on the subscription
            HttpClient client = new HttpClient();

            // Pass in the values
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("IsActive", isActive.ToString()),
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(ApiBaseUri + "subscription/" + subscriptionId.ToString()),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API
            var result = await client.SendAsync(requestMsg);

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> SetSubscriptionIsCommsOkay(int subscriptionId, bool isCommsOkay)
        {
            // Change the communications okay setting on the subscription
            HttpClient client = new HttpClient();

            // Pass in the values
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("IsCommsOkay", isCommsOkay.ToString()),
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(ApiBaseUri + "subscription/comms/" + subscriptionId.ToString()),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API
            var result = await client.SendAsync(requestMsg);

            return result.IsSuccessStatusCode;
        }

        public async Task<Subscription> GetSubscription(string userId)
        {
            // Get Subscription object from API
            Subscription subscription = new Subscription();

            // Call the API to get a Subscription object
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            string uri = "subscription?userId=" + userId;

            HttpResponseMessage response = await client.GetAsync(uri);

            // If successful then return the subscription
            if (response.IsSuccessStatusCode)
            {
                subscription = await response.Content.ReadAsAsync<Subscription>();
            }

            return subscription;
        }

        public async Task<bool> IsValidSubscription(string userId)
        {
            // Does the user have a valid subscription
            bool isValid = false;

            // Call the API to get a Subscription object
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            string uri = "subscription/valid?userId=" + userId;

            HttpResponseMessage response = await client.GetAsync(uri);

            // If successful then return the subscription
            if (response.IsSuccessStatusCode)
            {
                isValid = await response.Content.ReadAsAsync<bool>();
            }

            return isValid;
        }

        public async Task<bool> IsGDPROptIn(string userId)
        {
            // Has the user actively complied with GDPR
            bool isGDPROptIn = false;

            // Call the API to get a Subscription object
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            string uri = "subscription/gdpr?userId=" + userId;

            HttpResponseMessage response = await client.GetAsync(uri);

            // If successful then return the subscription
            if (response.IsSuccessStatusCode)
            {
                isGDPROptIn = await response.Content.ReadAsAsync<bool>();
            }

            return isGDPROptIn;
        }

        public async Task<bool> SetSubscriptionEnquiryAllocation(int subscriptionId, int enquiryAllocation)
        {
            // Change the number of enquiries allocated on the subscription
            HttpClient client = new HttpClient();

            // Pass in the values
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("EnquiryAllocation", enquiryAllocation.ToString()),
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(ApiBaseUri + "subscription/enquiry/" + subscriptionId.ToString()),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API
            var result = await client.SendAsync(requestMsg);

            return result.IsSuccessStatusCode;
        }
    }
}
