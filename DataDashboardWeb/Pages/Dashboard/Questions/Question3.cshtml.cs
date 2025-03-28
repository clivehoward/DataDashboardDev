﻿using System;
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
    public class Question3Model : PageModel
    {
        public ChartData ChartRawData { get; set; }
        public ChartJS Chart1 { get; set; }
        public ChartJS Chart2 { get; set; }
        public ChartJS Chart3 { get; set; }

        private readonly AppSettings _appSettings;

        public Question3Model(IOptions<AppSettings> appSettings)
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
            ChartRawData = await dataAccess.GetChartData(3, location, companySize, industry);
            if (ChartRawData.TotalResults > 0)
            {
                Chart1 = dataAccess.GetChartJSData(ChartRawData, 3, "Unimportant");
                Chart2 = dataAccess.GetChartJSData(ChartRawData, 3, "Important");
                Chart3 = dataAccess.GetChartJSData(ChartRawData, 3, "Essential");
            }

            return Page();
        }
    }
}