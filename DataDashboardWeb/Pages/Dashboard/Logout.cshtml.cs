using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using DataDashboardWebLib;
using Microsoft.AspNetCore.Http;

namespace DataDashboardWeb.Pages.Dashboard
{
    public class LogoutModel : PageModel
    {
        private readonly AppSettings _appSettings;

        public LogoutModel(IOptions<AppSettings> appSettings)
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

            string apiUrl = _appSettings.ApiUrl;

            // Logout the user from the API
            UserAccess userAccess = new UserAccess(apiUrl, token);
            userAccess.Logout();

            // Now, clear the session
            HttpContext.Session.Clear();

            return Redirect("/Login");
        }
    }
}