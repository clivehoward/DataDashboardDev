using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataDashboardMessagingLibCore;
using DataDashboardMessagingLibCore.Models;
using DataDashboardWebLib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace DataDashboardWeb.Pages.Dashboard
{
    public class EnquiryModel : PageModel
    {
        [BindProperty]
        public EnquiryFormModel EnquiryForm { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public bool IsEnquiryAvailable { get; set; } = true;

        public List<string> TopicAreas { get; set; }
        public List<string> Geographies { get; set; }
        public List<string> Industries { get; set; }
        public List<string> CompanySizes { get; set; }

        private readonly AppSettings _appSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDistributedCache _distributedCache;

        public EnquiryModel(IOptions<AppSettings> appSettings, IHostingEnvironment hostingEnvironment, IDistributedCache distributedCache)
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

            // NOTE //////////////////////////////////////////////////////////////////////////////
            // Get the User ID - DO NOT LIKE THIS!!!!
            // Would prefer not to pass the id and have it handled in the API using authentication
            // Or inside the Class Library (DataDashboardWebLib)
            string userId = await userAccess.GetUserId(user.Email);

            // Now, look up the subscription
            SubscriptionAccess subscriptionAccess = new SubscriptionAccess(apiUrl, token);
            Subscription subscription = await subscriptionAccess.GetSubscription(userId);

            if (subscription.EnquiryAllocation > 0)
            {
                IsEnquiryAvailable = true;

                EnquiryForm = new EnquiryFormModel()
                {
                    SubscriptionId = subscription.SubscriptionId, // Not sure if this should be in a hidden field !!!!
                    EnquiryAllocated = subscription.EnquiryAllocation,
                    UserFullName = user.FirstName + " " + user.LastName,
                    UserEmail = user.Email
                };
            }

            LoadDataFilters();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Create an email using the form data
            string azureStorageConnection = _appSettings.AzureStorageConnection; // Get the connection string for Azure Storage Queues

            // Get data from our form
            string fullName = EnquiryForm.UserFullName;
            string email = EnquiryForm.UserEmail;
            string topicArea = EnquiryForm.TopicArea;
            string geography = EnquiryForm.Geography;
            string industry = EnquiryForm.Industry;
            string companySize = EnquiryForm.CompanySize;

            int subscriptionId = EnquiryForm.SubscriptionId;
            int enquiryAllocated = EnquiryForm.EnquiryAllocated;

            // Path to the email template
            string webRootPath = _hostingEnvironment.WebRootPath;
            string emailTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                        + "templates" + Path.DirectorySeparatorChar.ToString()
                                        + "email" + Path.DirectorySeparatorChar.ToString()
                                        + "SubscriberEnquiryFormMessage.html";

            try
            {
                // Email time
                EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

                // Create the email content
                string body = emailAccess.CreateMessageBody(emailTemplatePath, fullName, topicArea, geography, industry, companySize);

                // Create the outgoing email object
                OutgoingEmail outgoingEmail = emailAccess.CreateOutgoingEmail("help@discoveredinsights.com", email, "Subscriber Enquiry Form Submission", body);

                // Send the email
                await emailAccess.Send(outgoingEmail);

                // Reduce the number of available enquiries for this subcription
                enquiryAllocated = enquiryAllocated - 1;

                var token = HttpContext.Session.GetString("Token");
                string apiUrl = _appSettings.ApiUrl;
                SubscriptionAccess subscriptionAccess = new SubscriptionAccess(apiUrl, token);
                await subscriptionAccess.SetSubscriptionEnquiryAllocation(subscriptionId, enquiryAllocated);

                if (enquiryAllocated > 0)
                {
                    IsEnquiryAvailable = true;
                }

                SuccessMessage = "Thank you for your enquiry";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Sorry, something went wrong and your enquiry was not sent";
            }

            LoadDataFilters();

            return Page();
        }

        private void LoadDataFilters()
        {
            TopicAreas = new List<string>(new string[]
            {
                "Digital Transformation",
                "Development strategy",
                "Technology and methodology",
                "Development languages",
                "Professional developers",
                "Non-professional developers",
                "Mobile development strategy",
                "Mobile app strategy",
                "Mobile technology strategy",
                "Cloud",
                "Application strategy"
            });

            Geographies = new List<string>(new string[]
            {
                "Asia Pacific", 
                "EMEA",
                "United Kingdom",
                "Rest of EMEA", 
                "North America",
                "South America"  
            });

            Industries = new List<string>(new string[]
            {
                "Chemicals and Petroleum",
                "Consultancies and Professional Services",
                "Financial Services",
                "Government (defence, education, health, utils)",
                "Manufacturing, Construction and Engineering",
                "Media, Entertainment and Gaming",
                "Retail and Consumer Goods",
                "Software",
                "Telecommunications",
                "Transport and Logistics"
            });

            CompanySizes = new List<string>(new string[]
            {
                "Less than 1000 employees",
                "1,000 – 25,000 employees",
                "More than 25,000 employees"
            });
        }
    }

    public class EnquiryFormModel
    {
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public int EnquiryAllocated { get; set; }
        public int SubscriptionId { get; set; }

        [AtLeastOneRequired("TopicArea,Geography,Industry,CompanySize", ErrorMessage = "Please select at least one of the options")]
        public string TopicArea { get; set; }
        public string Geography { get; set; }
        public string Industry { get; set; }
        public string CompanySize { get; set; }
    }
}