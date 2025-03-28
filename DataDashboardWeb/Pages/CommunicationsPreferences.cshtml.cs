using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DataDashboardWebLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace DataDashboardWeb.Pages
{
    public class CommunicationsPreferencesModel : PageModel
    {
        [BindProperty]
        public CommunicationsPreferencesFormModel CommunicationsPreferencesForm { get; set; }

        private readonly AppSettings _appSettings;

        public CommunicationsPreferencesModel(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IActionResult OnGet()
        {
            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            // If there is no token then send to login
            if (token == null)
            {
                return Redirect("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            string apiUrl = _appSettings.ApiUrl;

            // NOTE //////////////////////////////////////////////////////////////////////////////
            // This a bit messy, get the user and then get the Id separately!!
            // Get the user's account information
            UserAccess userAccess = new UserAccess(apiUrl, token);
            var user = await userAccess.GetUser();

            // NOTE //////////////////////////////////////////////////////////////////////////////
            // Get the User ID - DO NOT LIKE THIS!!!!
            // Would prefer not to pass the id and have it handled in the API using authentication
            // Or inside the Class Library (DataDashboardWebLib)
            string userId = await userAccess.GetUserId(user.Email);

            // Get the user's current subscription
            SubscriptionAccess subscriptionAccess = new SubscriptionAccess(apiUrl, token);
            Subscription subscription = await subscriptionAccess.GetSubscription(userId);

            // Now, set the communication preferences
            await subscriptionAccess.SetSubscriptionIsCommsOkay(subscription.SubscriptionId, CommunicationsPreferencesForm.IsOkay);

            return Redirect("/Dashboard/Home");
        }
    }

    public class CommunicationsPreferencesFormModel
    {
        [Required(ErrorMessage = "Please select one of the two options")]
        public bool IsOkay { get; set; }
    }
}