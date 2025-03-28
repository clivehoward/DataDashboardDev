using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataDashboardWebLib;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DataDashboardWeb.Pages
{
    public class LogInModel : PageModel
    {
        public string Message { get; set; }

        [BindProperty]
        public LoginFormModel LoginForm { get; set; }

        private readonly AppSettings _appSettings;

        public LogInModel(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IActionResult OnGet()
        {
            // If already authenticated then we're fine
            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            // If there is no token then send to login
            if (token != null)
            {
                return Redirect("/Dashboard/Home");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string redirectUrl = "";
            Message = "Sorry, you are not a subscriber."; // Default fail message

            // We need to authenticate against the API and get a bearer token
            string bearerToken = "";

            string apiUrl = _appSettings.ApiUrl;

            UserAccess userAccess = new UserAccess(apiUrl);
            bearerToken = await userAccess.GetBearerToken(LoginForm.Email, LoginForm.Password);

            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                // No bearer token and so login has failed
                Message = "Sorry, login failed. Please check your email and password.";
            }
            else
            {
                bool isOkay = false;

                // What roles is this user assigned?
                List<string> roles = await userAccess.GetUserRoles(bearerToken);

                if (roles.Exists(r => r == "administrator")) // If an admin then send to the admin screen
                {
                    redirectUrl = "/Administration/UserManagement";
                    isOkay = true;
                }
                if (roles.Exists(r => r == "sales")) // If sales person then go to their subscribe screen
                {
                    redirectUrl = "/Administration/UserManagement";
                    isOkay = true;
                }
                if (roles.Exists(r => r == "subscriber")) // Now check if the user is a subscriber
                {
                    isOkay = true;

                    // We have a token and so the login is valid but is there a valid subscription?
                    // First we need the user's ID
                    // NOTE: I don't like this. Would be better if we did not have to get the ID but it was handled in the API
                    // Perhaps using the authenticated user mechanism in the API to identify the user and then pass it to the 
                    // subscription library
                    string userId = await userAccess.GetUserId(LoginForm.Email);
                    if (string.IsNullOrWhiteSpace(userId) == false)
                    {
                        SubscriptionAccess subscriptionAccess = new SubscriptionAccess(apiUrl, bearerToken);
                        if (await subscriptionAccess.IsValidSubscription(userId))
                        {
                            // Login is fine, but have they complied with GDPR?
                            bool isGDPROptIn = await subscriptionAccess.IsGDPROptIn(userId);

                            if (isGDPROptIn == true)
                            {
                                redirectUrl = "/Dashboard/Home";
                            }
                            else
                            {
                                redirectUrl = "/CommunicationsPreferences";
                            }
                        }
                        else
                        {
                            Message = "Sorry, you do not have a currently active subscription.";
                        }
                    }
                    else
                    {
                        Message = "Sorry, you do not have a currently active subscription.";
                    }
                }

                // To use Session State it must be configured in Startup.cs
                if (isOkay)
                {
                    HttpContext.Session.SetString("Token", bearerToken);
                }
            }

            // If we have a redirect then use it, otherwise just return this page
            if (string.IsNullOrEmpty(redirectUrl))
            {
                return Page();
            }
            else
            {
                return Redirect(redirectUrl);
            }
        }
    }

    public class LoginFormModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,15}$", ErrorMessage = "Must contain 1 upper case, 1 lower case and 1 number")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}