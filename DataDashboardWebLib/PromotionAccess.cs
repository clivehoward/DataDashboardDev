using DataDashboardWebLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataDashboardWebLib
{
    public class PromotionAccess : IPromotionAccess
    {
        private string ApiBaseUri;

        public PromotionAccess(string apiBaseUri)
        {
            ApiBaseUri = apiBaseUri;
        }

        public async Task<bool> AddSubmission(string firstName, string lastName, string email, string promotionName, bool isCommsOkay)
        {
            // Add a new promotion submission
            bool isAdded = false;

            HttpClient client = new HttpClient();

            // Pass in the values
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("FirstName", firstName),
                new KeyValuePair<string, string>("LastName", lastName),
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("PromotionName", promotionName),
                new KeyValuePair<string, string>("IsCommsOkay", isCommsOkay.ToString()),
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "promotion/submission"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API and check is we're successful
            var result = await client.SendAsync(requestMsg);

            // Was the result successful?
            if (result.IsSuccessStatusCode)
            {
                isAdded = await result.Content.ReadAsAsync<bool>();
            }
            else
            {
                throw new Exception("Submission failed");
            }

            return isAdded;
        }
    }
}
