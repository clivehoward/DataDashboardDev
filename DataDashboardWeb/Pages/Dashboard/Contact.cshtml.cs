using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.IO;
using DataDashboardMessagingLibCore;
using System.ComponentModel.DataAnnotations;
using DataDashboardWebLib;
using Microsoft.AspNetCore.Http;
using DataDashboardMessagingLibCore.Models;

namespace DataDashboardWeb.Pages.Dashboard
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public SubscriberContactFormModel ContactForm { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        private readonly AppSettings _appSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDistributedCache _distributedCache;

        public ContactModel(IOptions<AppSettings> appSettings, IHostingEnvironment hostingEnvironment, IDistributedCache distributedCache)
        {
            _appSettings = appSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _distributedCache = distributedCache;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            // If there is no token then send to login
            if (token == null)
            {
                return Redirect("/Login");
            }

            string apiUrl = _appSettings.ApiUrl;

            // Get the user's account information
            UserAccess userAccess = new UserAccess(apiUrl, token);
            var user = await userAccess.GetUser();

            ContactForm = new SubscriberContactFormModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            // NOTE //////////////////////////////////////////////////////////////////////////////
            // Get the User ID - DO NOT LIKE THIS!!!!
            // Would prefer not to pass the id and have it handled in the API using authentication
            // Or inside the Class Library (DataDashboardWebLib)
            string userId = await userAccess.GetUserId(user.Email);

            // Now, look up the subscription
            SubscriptionAccess subscriptionAccess = new SubscriptionAccess(apiUrl, token);
            Subscription subscription = await subscriptionAccess.GetSubscription(userId);

            ContactForm.SubscriptionReference = subscription.SubscriptionReference; // Get the reference number

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Create an email using the form data
            string azureStorageConnection = _appSettings.AzureStorageConnection; // Get the connection string for Azure Storage Queues

            // Get data from our form
            string firstName = ContactForm.FirstName;
            string lastName = ContactForm.LastName;
            string email = ContactForm.Email;
            string message = ContactForm.EnquiryText;
            string reference = ContactForm.SubscriptionReference;

            // Path to the email template
            string webRootPath = _hostingEnvironment.WebRootPath;
            string emailTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                        + "templates" + Path.DirectorySeparatorChar.ToString()
                                        + "email" + Path.DirectorySeparatorChar.ToString()
                                        + "SubscriberContactFormMessage.html";

            try
            {
                // Email time
                EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

                // Create the email content
                string body = emailAccess.CreateMessageBody(emailTemplatePath, firstName, lastName, email, message, reference);

                // Create the outgoing email object
                OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail("help@discoveredinsights.com", email, "Subscriber Contact Form Submission", body);

                // Send the email
                await emailAccess.Send(outgoingEmail);

                SuccessMessage = "Thank you for your message";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Sorry, something went wrong and your message was not sent";
            }

            return Page();
        }
    }
    public class SubscriberContactFormModel
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

        [Required]
        [Display(Name = "Message")]
        public string EnquiryText { get; set; }

        public string SubscriptionReference { get; set; }
    }
}