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
using System.ComponentModel.DataAnnotations;
using DataDashboardMessagingLibCore;
using DataDashboardWebLib;
using DataDashboardMessagingLibCore.Models;

namespace DataDashboardWeb.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public PublicContactFormModel ContactForm { get; set; }

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

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Create an email using the form data
            string azureStorageConnection = _appSettings.AzureStorageConnection; // Get the connection string for Azure Storage Queues
            string reCaptchaKey = _appSettings.reCaptchaSecretKey; // Get reCAPTCHA secret key

            // Get values for use by reCAPTCHA
            string recaptchaResponse = Request.Form["g-recaptcha-response"]; // Hidden field
            string usersIPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(); // User's IP address

            // Get data from our form
            string firstName = ContactForm.FirstName;
            string lastName = ContactForm.LastName;
            string email = ContactForm.Email;
            string message = ContactForm.EnquiryText;

            // Path to the email template
            string webRootPath = _hostingEnvironment.WebRootPath;
            string emailTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                        + "templates" + Path.DirectorySeparatorChar.ToString()
                                        + "email" + Path.DirectorySeparatorChar.ToString()
                                        + "PublicContactFormMessage.html";

            // This is our transaction helper
            TransactionAccess transactionAccess = new TransactionAccess(""); // Expects an API URL but we don't need it in this case

            // Validate the reCAPTCHA widget, if fails then show message and stop
            ReCaptchaResult recaptchaResult = await transactionAccess.ValidateReCaptcha(reCaptchaKey, recaptchaResponse, usersIPAddress);
            if (recaptchaResult.IsSuccess == false)
            {
                ErrorMessage = recaptchaResult.ErrorMessage;
                return Page();
            }

            try
            {
                // Email time
                EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

                // Create the email content
                string body = emailAccess.CreateMessageBody(emailTemplatePath, firstName, lastName, email, message);

                // Create the outgoing email object
                OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail("help@discoveredinsights.com", email, "Public Contact Form Submission", body);

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
    public class PublicContactFormModel
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

        [Required]
        [Display(Name = "reCAPTCHA")]
        public string ReCaptcha { get; set; }
    }
}