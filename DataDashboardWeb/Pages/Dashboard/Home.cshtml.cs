using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using DataDashboardWebLib;

namespace DataDashboardWeb.Pages.Dashboard
{
    public class HomeModel : PageModel
    {
        public string FirstName { get; set; }

        private readonly AppSettings _appSettings;

        public HomeModel(IOptions<AppSettings> appSettings)
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

            // Get the user's account information in order to get their first name :)
            UserAccess userAccess = new UserAccess(apiUrl, token);
            var user = await userAccess.GetUser();

            FirstName = user.FirstName;

            return Page();
        }
    }
}