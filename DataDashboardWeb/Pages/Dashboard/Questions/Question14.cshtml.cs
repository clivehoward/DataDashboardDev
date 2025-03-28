using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using DataDashboardWebLib;

namespace DataDashboardWeb.Pages.Dashboard.Questions
{
    public class Question14Model : PageModel
    {
        public ChartData ChartRawData { get; set; }
        public ChartJS Chart1 { get; set; }
        public ChartJS Chart2 { get; set; }
        public ChartJS Chart3 { get; set; }
        public ChartJS Chart4 { get; set; }
        public ChartJS Chart5 { get; set; }

        private readonly AppSettings _appSettings;

        public Question14Model(IOptions<AppSettings> appSettings)
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

            string location = string.Empty;
            string companySize = string.Empty;
            string industry = string.Empty;

            // Let's see if any filters have been set...
            if (HttpContext.Session.GetString("Geographies") != null) location = HttpContext.Session.GetString("Geographies");
            if (HttpContext.Session.GetString("Industries") != null) industry = HttpContext.Session.GetString("Industries");
            if (HttpContext.Session.GetString("CompanySizes") != null) companySize = HttpContext.Session.GetString("CompanySizes");

            string apiUrl = _appSettings.ApiUrl;

            // Get our authentication token from session state

            // Get a ChartData object and then use it to generate objects for use by ChartJS
            DataAccess dataAccess = new DataAccess(apiUrl, token);
            ChartRawData = await dataAccess.GetChartData(14, location, companySize, industry);
            if (ChartRawData.TotalResults > 0)
            {
                Chart1 = dataAccess.GetChartJSData(ChartRawData, 14, "NotImportant");
                Chart2 = dataAccess.GetChartJSData(ChartRawData, 14, "ModeratelyImportant");
                Chart3 = dataAccess.GetChartJSData(ChartRawData, 14, "Important");
                Chart4 = dataAccess.GetChartJSData(ChartRawData, 14, "VeryImportant");
                Chart5 = dataAccess.GetChartJSData(ChartRawData, 14, "Crucial");
            }

            return Page();
        }
    }
}