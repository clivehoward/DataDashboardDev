using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using DataDashboardWebLib;

namespace DataDashboardWeb.Pages.Dashboard
{
    public class AccountModel : PageModel
    {
        [BindProperty]
        public AccountDetailsModel AccountForm { get; set; }

        [BindProperty]
        public ChangePasswordModel ChangePasswordForm { get; set; }

        [BindProperty]
        public CommunicationPreferencesModel CommunicationPreferencesForm { get; set; }

        public string SubscriptionDaysRemaining { get; set; }
        public string SubscriptionReference { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        private readonly AppSettings _appSettings;

        public AccountModel(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
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
            
            // Now populate the account form fields
            AccountForm = new AccountDetailsModel
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

            SubscriptionDaysRemaining = Math.Round((subscription.EndDate - DateTime.Now).TotalDays).ToString(); // How long left on this subscription
            SubscriptionReference = subscription.SubscriptionReference; // Get the reference number

            CommunicationPreferencesForm = new CommunicationPreferencesModel
            {
                IsOkay = subscription.IsCommsOkay
            };

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            // Update personal details
            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            string apiUrl = _appSettings.ApiUrl;

            // Update the user's details
            UserAccess userAccess = new UserAccess(apiUrl, token);
            bool isResult = await userAccess.UpdateUser(AccountForm.Email, AccountForm.FirstName, AccountForm.LastName);

            if (isResult)
            {
                SuccessMessage = "Update Successful";
            }
            else
            {
                ErrorMessage = "Update Unsuccessful";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostChangepasswordAsync()
        {
            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            string apiUrl = _appSettings.ApiUrl;

            // Change the user's password
            UserAccess userAccess = new UserAccess(apiUrl, token);
            bool isResult = await userAccess.ChangePassword(ChangePasswordForm.Password, ChangePasswordForm.NewPassword);

            if (isResult)
            {
                SuccessMessage = "Password Change Successful";
            }
            else
            {
                ErrorMessage = "Password Change Unsuccessful";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync()
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

            // Now, look up the subscription
            SubscriptionAccess subscriptionAccess = new SubscriptionAccess(apiUrl, token);
            Subscription subscription = await subscriptionAccess.GetSubscription(userId);

            // De-activate the subscription
            await subscriptionAccess.SetSubscriptionIsActive(subscription.SubscriptionId, false);

            // Delete the user
            await userAccess.Delete();

            // Now, clear the session
            HttpContext.Session.Clear();

            return Redirect("/Index");
        }

        public async Task<IActionResult> OnPostSavepreferencesAsync()
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
            bool isResult = await subscriptionAccess.SetSubscriptionIsCommsOkay(subscription.SubscriptionId, CommunicationPreferencesForm.IsOkay);

            if (isResult)
            {
                SuccessMessage = "Preferences Saved Successfully";
            }
            else
            {
                ErrorMessage = "Preferences Saved Unsuccessfully";
            }

            return Page();
        }
    }

    public class AccountDetailsModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,15}$", ErrorMessage = "Must contain 1 upper case, 1 lower case and 1 number")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
    }

    public class CommunicationPreferencesModel
    {
        [Required(ErrorMessage = "Please select one of the two options")]
        public bool IsOkay { get; set; }
    }
}