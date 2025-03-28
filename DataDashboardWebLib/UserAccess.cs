using DataDashboardWebLib.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataDashboardWebLib
{
    public class UserAccess : IUserAccess
    {
        private string ApiBaseUri;
        private string BearerToken;

        public UserAccess(string apiBaseUri)
        {
            ApiBaseUri = apiBaseUri;
        }

        public UserAccess(string apiBaseUri, string bearerToken)
        {
            ApiBaseUri = apiBaseUri;
            BearerToken = bearerToken;
        }

        public async Task<string> RegisterUser(string email, string password, string firstName, string lastName)
        {
            // Register a new user against the API
            string userId = string.Empty;

            HttpClient client = new HttpClient();

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("ConfirmPassword", password),
                new KeyValuePair<string, string>("FirstName", firstName),
                new KeyValuePair<string, string>("LastName", lastName)
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "api/Account/Register"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API and check is we're successful
            var result = await client.SendAsync(requestMsg);
            var resultData = await result.Content.ReadAsStringAsync();

            // If the result was successful then we want to get the new user's identifier
            if (result.IsSuccessStatusCode)
            {
                userId = await GetUserId(email); // Get the new user's ID
            }

            return userId;
        }

        public async Task<string> GetUserId(string email)
        {
            // Call the API to get a User ID based on their email
            string userId = string.Empty;

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("api/Account/UserInfo?email=" + email);

            // If we have a successful response then get the User's ID which should be in the response
            if (response.IsSuccessStatusCode)
            {
                userId = await response.Content.ReadAsAsync<string>();
            }

            return userId;
        }

        public async Task<UserInfo> GetUser()
        {
            // Call the API to get a User's details
            UserInfo user = new UserInfo();

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            HttpResponseMessage response = await client.GetAsync("api/Account/UserInfo");

            // If we have a successful response then get the User's details which should be in the response
            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadAsStringAsync();

                user.Email = JObject.Parse(userData)["Email"].ToString();
                user.FirstName = JObject.Parse(userData)["FirstName"].ToString();
                user.LastName = JObject.Parse(userData)["LastName"].ToString();
            }

            // Retur the user details in the UserInfo object
            return user;
        }

        public async Task<bool> ChangePassword(string oldPassword, string newPassword)
        {
            // Change password for authenticated user
            bool isSuccess = false;

            HttpClient client = new HttpClient();

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("OldPassword", oldPassword),
                new KeyValuePair<string, string>("NewPassword", newPassword),
                new KeyValuePair<string, string>("ConfirmPassword", newPassword)
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "api/Account/ChangePassword"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            // Hit the API and check if we're successful
            var result = await client.SendAsync(requestMsg);

            if (result.IsSuccessStatusCode)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        public async Task<bool> UpdateUser(string email, string firstName, string lastName)
        {
            // Update a user against the API
            bool isSuccess = false;

            HttpClient client = new HttpClient();

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("FirstName", firstName),
                new KeyValuePair<string, string>("LastName", lastName)
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "api/Account/Update"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            // Hit the API and check is we're successful
            var result = await client.SendAsync(requestMsg);

            // If the result was successful then we're all good
            if (result.IsSuccessStatusCode)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        public async void Logout()
        {
            // Logout of API
            HttpClient client = new HttpClient();

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "api/Account/Logout")
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            await client.SendAsync(requestMsg);
        }

        public async Task Delete()
        {
            // Delete the currently authenticated user
            HttpClient client = new HttpClient();

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(ApiBaseUri + "api/Account/Delete")
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            await client.SendAsync(requestMsg);
        }

        public async Task Delete(string userId)
        {
            // NOTE //////////////////////////////////////////////////////////////////////////////
            // Requires User ID - DO NOT LIKE THIS!!!!
            // Would prefer not to pass the id and perhaps use email instead and then look-up user
            // here on in the API

            // Delete user by Id
            HttpClient client = new HttpClient();

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(ApiBaseUri + "api/Account/Delete/" + userId)
            };

            await client.SendAsync(requestMsg);
        }

        public async Task AddToRole(string userId, string roleName)
        {
            // NOTE //////////////////////////////////////////////////////////////////////////////
            // Requires User ID - DO NOT LIKE THIS!!!!
            // Would prefer not to pass the id and perhaps use email instead and then look-up user
            // here on in the API

            HttpClient client = new HttpClient();

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Role", roleName)
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "api/Account/AddToRole/" + userId),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            await client.SendAsync(requestMsg);
        }

        public async Task<List<string>> GetUserRoles(string bearerToken)
        {
            BearerToken = bearerToken;

            // Get a list of role names that the user belongs to
            List<string> roles = new List<string>();

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            HttpResponseMessage response = await client.GetAsync("api/Account/GetRoles");

            // If we have a successful response then get the User's roles
            if (response.IsSuccessStatusCode)
            {
                roles = await response.Content.ReadAsAsync<List<string>>();
            }

            return roles;
        }

        public async Task<string> GetBearerToken(string email, string password)
        {
            // We need to get a bearer token from the API
            // This will allow us to authenticate future calls
            string bearerToken = string.Empty;

            HttpClient client = new HttpClient();

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "Token"),
                Content = new StringContent($"grant_type=password&username={email}&password={password}")
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API and hope to get a token in return
            var bearerResult = await client.SendAsync(requestMsg);
            var bearerData = await bearerResult.Content.ReadAsStringAsync();

            // If the result was successful then get the token or throw an exception
            if (bearerResult.IsSuccessStatusCode)
            {
                bearerToken = JObject.Parse(bearerData)["access_token"].ToString();
            }

            return bearerToken;
        }

        public async Task<bool> ForgotPassword(string email)
        {
            // Send user a token to reset their password
            bool isSuccess = false;

            HttpClient client = new HttpClient();

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Email", email),
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "api/Account/ForgotPassword"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API and check if we're successful
            var result = await client.SendAsync(requestMsg);

            if (result.IsSuccessStatusCode)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        public async Task<bool> ResetPassword(string email, string code, string password)
        {
            // Reset the user's password
            bool isSuccess = false;

            HttpClient client = new HttpClient();

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("Code", code),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("ConfirmPassword", password)
            };

            // Build the request
            var requestMsg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiBaseUri + "api/Account/ResetPassword"),
                Content = new FormUrlEncodedContent(values)
            };

            requestMsg.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

            // Hit the API and check if we're successful
            var result = await client.SendAsync(requestMsg);

            if (result.IsSuccessStatusCode)
            {
                isSuccess = true;
            }

            return isSuccess;
        }
    }
}
