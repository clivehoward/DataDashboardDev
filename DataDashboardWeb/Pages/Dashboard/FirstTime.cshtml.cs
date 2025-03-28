using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace DataDashboardWeb.Pages.Dashboard
{
    public class FirstTimeModel : PageModel
    {
        private readonly AppSettings _appSettings;

        public FirstTimeModel(IOptions<AppSettings> appSettings)
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
    }
}