using DataDashboardWebLib;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataDashboardWeb.Pages
{
    public class CoveragePercentage
    {
        public static async Task<string> GetPercentage(HttpContext context, IOptions<AppSettings> appSettings)
        {
            // Get the level of coverage given specified filters (no filters, then ignore)
            // This tells users what percentage of questions have answers
            string html = "";

            string location = string.Empty;
            string companySize = string.Empty;
            string industry = string.Empty;

            // Let's see if any filters have been set...
            if (context.Session.GetString("Geographies") != null) location = context.Session.GetString("Geographies");
            if (context.Session.GetString("Industries") != null) industry = context.Session.GetString("Industries");
            if (context.Session.GetString("CompanySizes") != null) companySize = context.Session.GetString("CompanySizes");

            if (string.IsNullOrEmpty(location) == false || string.IsNullOrEmpty(industry) == false || string.IsNullOrEmpty(companySize) == false)
            {
                string apiUrl = appSettings.Value.ApiUrl;

                // Get our authentication token from session state
                var token = context.Session.GetString("Token");

                // Get a ChartData object and then use it to generate objects for use by ChartJS
                DataAccess dataAccess = new DataAccess(apiUrl, token);
                int coverage = await dataAccess.GetCoverage(location, companySize, industry);

                html = string.Format("<span class=\"badge\" style=\"background-color: white; color:#008cba\" title=\"{0}% of questions have data available for the filters selected\">{0}%</span>", coverage.ToString());
            }

            return html;
        }
    }
}
