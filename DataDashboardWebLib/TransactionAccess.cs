using DataDashboardWebLib.Helpers;
using DataDashboardWebLib.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataDashboardWebLib
{
    public class TransactionAccess : ITransactionAccess
    {
        private string ApiBaseUri;

        public TransactionAccess(string apiBaseUri)
        {
            ApiBaseUri = apiBaseUri;
        }

        public async Task<ReCaptchaResult> ValidateReCaptcha(string reCaptchaKey, string response, string remoteip)
        {
            ReCaptchaResult rcResult = new ReCaptchaResult()
            {
                IsSuccess = false // default to fail
            };

            // https://www.google.com/recaptcha/admin#site/339298854?setup
            // Validate reCAPTCHA...
            HttpClient client = new HttpClient();

            // Add the paramters
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("secret", reCaptchaKey),
                new KeyValuePair<string, string>("response", response),
                new KeyValuePair<string, string>("remoteip", remoteip)
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://www.google.com/recaptcha/api/siteverify"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API and hope to get a token in return
            var result = await client.SendAsync(requestMsg);
            var resultData = await result.Content.ReadAsStringAsync();

            // If the result was successful then check if the transaction was successful
            if (result.IsSuccessStatusCode)
            {
                string success = JObject.Parse(resultData)["success"].ToString();
                if (success.ToLower() == "true")
                {
                    rcResult.IsSuccess = true;
                }
                else
                {
                    rcResult.ErrorMessage = "Sorry, we think that you may be a robot"; //JObject.Parse(resultData)["error-codes"].ToString();
                }
            }
            else
            {
                rcResult.ErrorMessage = result.ReasonPhrase;
            }

            return rcResult;
        }

        public async Task<UserResult> CreateNewUser(string email, string firstName, string lastName, string roleName)
        {
            UserResult result = new UserResult()
            {
                IsSuccess = false // default to failure
            };

            string userId = "";

            // Generate a new password
            string password = Password.Generate();

            // Now create the user
            UserAccess userAccess = new UserAccess(ApiBaseUri);
            userId = await userAccess.RegisterUser(email, password, firstName, lastName);

            // If we have done that successfully then create a new subscription
            if (string.IsNullOrWhiteSpace(userId) == false)
            {
                result.UserId = userId;
                result.Password = password;
                result.IsSuccess = true;
            }
            else
            {
                result.ErrorMessage = "User creation failed.";
            }

            // Before we're done, we add this user to a role if there is one
            if (string.IsNullOrWhiteSpace(roleName) == false)
            {
                await userAccess.AddToRole(userId, roleName);
            }

            return result;
        }

        public async Task<SubscriptionResult> CreateNewSubscription(string userId, string paymentReference, decimal amount, 
                                                                    bool isGDPROptIn, bool isCommsOkay, int enquiryAllocation)
        {
            SubscriptionResult result = new SubscriptionResult()
            {
                IsSuccess = false // default to failure
            };

            int subscriptionId = 0; // THIS IS WHAT WE NEED IN ORDER TO SHOW ON NEXT SCREEN !! PROBABLY ALSO ADD TO EMAIL SUBJECT
            string subscriptionReference = GenerateSubscriptionReference();

            SubscriptionAccess subscriptionAccess = new SubscriptionAccess(ApiBaseUri);
            subscriptionId = await subscriptionAccess.CreateSubscription(userId, paymentReference, amount, subscriptionReference, isGDPROptIn, isCommsOkay, enquiryAllocation);

            // If subscription was created then all good
            if (subscriptionId > 0)
            {
                result.SubscriptionId = subscriptionId;
                result.SubscriptionReference = subscriptionReference;
                result.IsSuccess = true;
            }
            else
            {
                result.ErrorMessage = "Subscription failed.";
            }

            return result;
        }

        private string GenerateSubscriptionReference()
        {
            // Generate a unique subscription reference
            string reference = string.Empty;

            // Use the date and some random letters
            int hash = (Math.Abs(DateTime.Now.GetHashCode()))/1000;

            // Add a couple of random letters
            Random random = new Random();
            int randomNumber = 0;

            randomNumber = random.Next(0, 26);
            string ltr1 = ((char)('A' + randomNumber)).ToString();
            randomNumber = random.Next(0, 26);
            string ltr2 = ((char)('A' + randomNumber)).ToString();

            reference = ltr1 + ltr2 + hash.ToString();

            return reference;
        }
    }
}
