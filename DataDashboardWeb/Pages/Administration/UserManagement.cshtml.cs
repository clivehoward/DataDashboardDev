using DataDashboardMessagingLibCore;
using DataDashboardMessagingLibCore.Models;
using DataDashboardWebLib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DataDashboardWeb.Pages.Administration
{
    public class UserManagementModel : PageModel
    {
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public UserFormModel UserForm { get; set; }
        [BindProperty]
        public SubscriptionFormModel SubscriptionForm { get; set; }
        [BindProperty]
        public DeleteUserFormModel DeleteUserForm { get; set; }

        private readonly AppSettings _appSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDistributedCache _distributedCache;

        public UserManagementModel(IOptions<AppSettings> appSettings, IHostingEnvironment hostingEnvironment, IDistributedCache distributedCache)
        {
            _appSettings = appSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _distributedCache = distributedCache;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            // Check that user is an admin
            string apiUrl = _appSettings.ApiUrl;
            UserAccess userAccess = new UserAccess(apiUrl);

            List<string> roles = await userAccess.GetUserRoles(token);

            if (roles.Exists(r => r == "administrator") == false) // If not an admin then send to login screen
            {
                return Redirect("/Login");
            }
            else
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCreateUserAsync()
        {
            var user = await CreateNewUser(); // Create a new user

            string firstName = UserForm.FirstName;
            string email = UserForm.Email;
            string password = user.Password;

            await SendNewUserEmail(firstName, email, password); // Send them a welcome email

            return Page();
        }

        public async Task<IActionResult> OnPostCreateSubscriptionAsync()
        {
            // Create a new subscription
            await CreateNewSubscription();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateUserAndSubscriptionAsync()
        {
            // Create new user and subscription and send email
            var user = await CreateNewUser();
            var subscription = await CreateNewSubscription();

            string firstName = UserForm.FirstName;
            string email = UserForm.Email;
            string password = user.Password;
            string subscriptionReference = subscription.SubscriptionReference;

            decimal? paymentAmount = SubscriptionForm.PaymentAmount;
            if (paymentAmount is null)
            {
                paymentAmount = 0;
            }

            await SendNewSubscriptionEmail(firstName, paymentAmount.ToString(), email, password, subscriptionReference);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync()
        {
            // Delete the user
            string apiUrl = _appSettings.ApiUrl; // Get URL of our API

            string email = DeleteUserForm.Email;

            // First, check to see if there is a user
            UserAccess userAccess = new UserAccess(apiUrl);
            string userId = await userAccess.GetUserId(email);

            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "User not found";
            }
            else
            {
                await userAccess.Delete(userId);
                SuccessMessage = "User deleted";
            }

            return Page();
        }

        private async Task<UserResult> CreateNewUser()
        {
            // Create a new user assigned to a specific role
            string apiUrl = _appSettings.ApiUrl; // Get URL of our API

            // Get data from our form
            string firstName = UserForm.FirstName;
            string lastName = UserForm.LastName;
            string email = UserForm.Email;
            string role = UserForm.Role;

            UserResult result = new UserResult();

            // This is our transaction helper
            TransactionAccess transactionAccess = new TransactionAccess(apiUrl);
            result = await transactionAccess.CreateNewUser(email, firstName, lastName, role);
            if (result.IsSuccess == false)
            {
                ErrorMessage = result.ErrorMessage;
            }
            else
            {
                SuccessMessage = "User Created";
            }

            return result;
        }

        private async Task<SubscriptionResult> CreateNewSubscription()
        {
            // Create a new subscription and assign to user
            string apiUrl = _appSettings.ApiUrl; // Get URL of our API

            string email = SubscriptionForm.Email;
            string paymentReference = SubscriptionForm.PaymentReference;
            decimal? paymentAmount = SubscriptionForm.PaymentAmount;
            int enquiryAllocation = (int)SubscriptionForm.EnquiryAllocation;

            if (paymentAmount is null)
            {
                paymentAmount = 0;
            }

            SubscriptionResult result = new SubscriptionResult();

            // First, check to see if there is a user
            UserAccess userAccess = new UserAccess(apiUrl);
            string userId = await userAccess.GetUserId(email);

            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "User not found";
            }
            else
            {
                // This is our transaction helper
                TransactionAccess transactionAccess = new TransactionAccess(apiUrl);
                result = await transactionAccess.CreateNewSubscription(userId, paymentReference, (decimal)paymentAmount, false, false, enquiryAllocation);
                if (result.IsSuccess == false)
                {
                    ErrorMessage = result.ErrorMessage;
                }
                else
                {
                    SuccessMessage = "Subscription created";
                }
            }

            return result;
        }

        private async Task SendNewSubscriptionEmail(string firstName, string amountPaid, string email, string password, string subscriptionReference)
        {
            // Create and send a new subscriber email
            string azureStorageConnection = _appSettings.AzureStorageConnection; // Get the connection string for Azure Storage Queues

            string webRootPath = _hostingEnvironment.WebRootPath;
            string emailTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                        + "templates" + Path.DirectorySeparatorChar.ToString()
                                        + "email" + Path.DirectorySeparatorChar.ToString()
                                        + "NewSubscriptionTemplate.html";

            string subject = "Welcome to Discovered Insights - " + subscriptionReference;

            EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

            // Retrofit tax and pre-tax amounts
            // NOTE: This is probably NOT the best way to do this !!!!!
            decimal taxRate = 0.2M;
            int preTaxtAmount = Convert.ToInt32((Convert.ToDecimal(amountPaid) / ((taxRate*100)+100))*100);
            int taxAmount = Convert.ToInt32(amountPaid) - preTaxtAmount;

            string body = emailAccess.CreateMessageBody(emailTemplatePath, firstName, password, amountPaid, subscriptionReference, 
                                                        preTaxtAmount.ToString(), (Convert.ToInt32(taxRate * 100)).ToString(), taxAmount.ToString());

            OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail(email, "no-reply@discoveredinsights.com", subject, body);

            await emailAccess.Send(outgoingEmail);
        }

        private async Task SendNewUserEmail(string firstName, string email, string password)
        {
            // Create and send a new user email
            string azureStorageConnection = _appSettings.AzureStorageConnection; // Get the connection string for Azure Storage Queues

            string webRootPath = _hostingEnvironment.WebRootPath;
            string emailTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                        + "templates" + Path.DirectorySeparatorChar.ToString()
                                        + "email" + Path.DirectorySeparatorChar.ToString()
                                        + "NewUserTemplate.html";

            string subject = "Welcome to Discovered Insights";

            EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

            string body = emailAccess.CreateMessageBody(emailTemplatePath, firstName, password);

            OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail(email, "no-reply@discoveredinsights.com", subject, body);

            await emailAccess.Send(outgoingEmail);
        }
    }

    public class UserFormModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class SubscriptionFormModel
    {
        public string Email { get; set; }
        public string PaymentReference { get; set; }
        public decimal? PaymentAmount { get; set; }
        public int? EnquiryAllocation { get; set; }
    }

    public class DeleteUserFormModel
    {
        public string Email { get; set; }
    }
}