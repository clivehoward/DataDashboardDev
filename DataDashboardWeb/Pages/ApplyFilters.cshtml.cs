using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace DataDashboardWeb.Pages
{
    public class ApplyFiltersModel : PageModel
    {
        public string Message { get; set; }
        public List<DataFilter> Geographies { get; set; }
        public List<DataFilter> Industries { get; set; }
        public List<DataFilter> CompanySizes { get; set; }

        public void OnGet()
        {
            /*
            CHANGE:
            (9-5-2018)

            To reduce the number of filters the Values property has been added to the DataFilter class
            This allows a single option to represent multiple actual filter options
            For example, "Small" organisation size can include filter options 1,2,3,4
            Previously the Id property was used
            To make this work, the Ids have been changed so that they are unique across ALL filters
            Instead of only unique within a specific filter

            As a result, we have to hold each filter in two session variables:
            One holds IDs for checking filter options when this screen loads
            Two holds the actual values that get passed to the API

            -- Original Geographies, Industries and CompanySizes can be found at the bottom of this file --

            */

            LoadDataFilters();

            // Now, if we have any filters set let's assign them
            CheckExistingFilters(Geographies);
            CheckExistingFilters(Industries);
            CheckExistingFilters(CompanySizes);
        }

        private void LoadDataFilters()
        {
            Geographies = new List<DataFilter>()
            {
                new DataFilter() { Id = 100, Values = "1", Name = "Asia Pacific", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 101, Values = "3", Name = "EMEA", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 102, Values = "29", Name = "United Kingdom", Indent = 1, IsSelected = false },
                new DataFilter() { Id = 103, Values = "14,15,16,17,19,20,7,23,24,26,27,28", Name = "Rest of EMEA", Indent = 1, IsSelected = false },
                new DataFilter() { Id = 104, Values = "4", Name = "North America", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 105, Values = "5", Name = "South America", Indent = 0, IsSelected = false }
            };

            Industries = new List<DataFilter>()
            {
                new DataFilter() { Id = 200, Values = "3", Name = "Chemicals and Petroleum", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 201, Values = "4", Name = "Consultancies and Professional Services", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 202, Values = "8", Name = "Financial Services", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 203, Values = "9,1,5,6,10", Name = "Government (defence, education, health, utils)", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 204, Values = "12,2", Name = "Manufacturing, Construction and Engineering", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 205, Values = "13", Name = "Media, Entertainment and Gaming", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 206, Values = "7,14", Name = "Retail and Consumer Goods", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 207, Values = "11", Name = "Software", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 208, Values = "15", Name = "Telecommunications", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 209, Values = "16", Name = "Transport and Logistics", Indent = 0, IsSelected = false }
            };

            CompanySizes = new List<DataFilter>()
            {
                new DataFilter() { Id = 300, Values = "1,2,3,4", Name = "Less than 1000 employees", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 301, Values = "5,6", Name = "1,000 – 25,000 employees", Indent = 0, IsSelected = false },
                new DataFilter() { Id = 302, Values = "7", Name = "More than 25,000 employees", Indent = 0, IsSelected = false }
            };
        }

        public IActionResult OnPostApply()
        {
            // Get the selected filters (if any)
            // This gives us the IDs, not the actual values
            string geographies = Request.Form["chkgeography"].ToString();
            string industries = Request.Form["chkindustry"].ToString();
            string companySizes = Request.Form["chkcompanysize"].ToString();

            // Get the page that we need to postback to
            string postbackPage = Request.Form["postbackPage"];

            // Save the Ids for use when/if this page is re-opened (to preselect checkboxes)
            List<int> Ids = new List<int>();
            if (string.IsNullOrWhiteSpace(geographies) == false) Ids.AddRange(geographies.Split(',').Select(int.Parse).ToList());
            if (string.IsNullOrWhiteSpace(industries) == false) Ids.AddRange(industries.Split(',').Select(int.Parse).ToList());
            if (string.IsNullOrWhiteSpace(companySizes) == false) Ids.AddRange(companySizes.Split(',').Select(int.Parse).ToList());
            HttpContext.Session.SetString("Filters", string.Join(",", Ids.ToArray())); // Save to session as comma separated string

            // Convert IDs into values
            LoadDataFilters();

            geographies = GetValues(geographies, Geographies);
            industries = GetValues(industries, Industries);
            companySizes = GetValues(companySizes, CompanySizes);

            // Now, assign these to our session
            HttpContext.Session.SetString("Geographies", geographies);
            HttpContext.Session.SetString("Industries", industries);
            HttpContext.Session.SetString("CompanySizes", companySizes);

            if (string.IsNullOrWhiteSpace(postbackPage))
            {
                postbackPage = "DashboardHome";
            }

            return Redirect(postbackPage);
        }

        public IActionResult OnPostClear()
        {
            // Clear all filters
            HttpContext.Session.SetString("Geographies", string.Empty);
            HttpContext.Session.SetString("Industries", string.Empty);
            HttpContext.Session.SetString("CompanySizes", string.Empty);
            HttpContext.Session.SetString("Filters", string.Empty);

            // Get the page that we need to postback to
            string postbackPage = Request.Form["postbackPage"];

            if (string.IsNullOrWhiteSpace(postbackPage))
            {
                postbackPage = "DashboardHome";
            }

            return Redirect(postbackPage);
        }

        private void CheckExistingFilters(List<DataFilter> dataFilters)
        {
            // This method was changed as part of the 9-5-18 change
            // Check to see if any filters have already been set
            // If so then make sure they are checked when the form loads
            string filters = string.Empty;

            if (HttpContext.Session.GetString("Filters") != null) filters = HttpContext.Session.GetString("Filters");

            string[] ids;

            if (string.IsNullOrWhiteSpace(filters) == false)
            {
                ids = filters.Split(',');
                foreach (string id in ids)
                {
                    foreach (var item in dataFilters)
                    {
                        if (id == item.Id.ToString())
                        {
                            item.IsSelected = true;
                        }
                    }
                }
            }
        }

        private string GetValues(string filter, List<DataFilter> dataFilters)
        {
            // Convert a list of Ids into a list of Values
            string values = string.Empty;
            string[] ids;

            if (string.IsNullOrWhiteSpace(filter) == false)
            {
                ids = filter.Split(',');
                foreach (string id in ids)
                {
                    foreach (var item in dataFilters)
                    {
                        if (id == item.Id.ToString())
                        {
                            values = values + item.Values + ",";
                        }
                    }
                }

                values = values.Substring(0, values.Length - 1); // strip the last ','
            }

            return values;
        }
    }

    public class DataFilter
    {
        public int Id { get; set; }
        public string Values { get; set; }
        public string Name { get; set; }
        public int Indent { get; set; }
        public bool IsSelected { get; set; }
    }
}

/*
Geographies = new List<DataFilter>()
{
    new DataFilter() { Id = 1, Values = "1", Name = "Asia Pacific", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 2, Values = "2", Name = "Australia", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 13, Values = "13", Name = "China", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 18, Values = "18", Name = "India", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 21, Values = "21", Name = "Japan", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 25, Values = "25", Name = "South Korea", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 3, Values = "3", Name = "EMEA", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 14, Values = "14", Name = "Denmark", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 15, Values = "15", Name = "Finland", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 16, Values = "16", Name = "France", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 17, Values = "17", Name = "Germany", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 19, Values = "19", Name = "Ireland", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 20, Values = "20", Name = "Italy", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 7, Values = "7", Name = "Middle East/Africa", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 23, Values = "23", Name = "Norway", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 24, Values = "24", Name = "Russia", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 26, Values = "26", Name = "Spain", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 27, Values = "27", Name = "Sweden", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 28, Values = "28", Name = "Switzerland", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 29, Values = "29", Name = "United Kingdom", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 4, Values = "4", Name = "North America", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 12, Values = "12", Name = "Canada", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 30, Values = "30", Name = "United States of America", Indent = 1, IsSelected = false },
    new DataFilter() { Id = 5, Values = "5", Name = "South America", Indent = 0, IsSelected = false }          
};

Industries = new List<DataFilter>()
{
    new DataFilter() { Id = 1, Values = "1", Name = "Aerospace and Defence", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 2, Values = "2", Name = "Automotive", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 3, Values = "3", Name = "Chemicals and Petroleum", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 4, Values = "4", Name = "Consultancies and Professional Services", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 5, Values = "5", Name = "Education", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 6, Values = "6", Name = "Energy and Utilities", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 7, Values = "7", Name = "Consumer Goods", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 8, Values = "8", Name = "Financial Services (Banking, Insurance)", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 9, Values = "9", Name = "Government and Public Sector", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 10, Values = "10", Name = "Healthcare, Life Sciences, and Biotech", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 11, Values = "11", Name = "Software", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 12, Values = "12", Name = "Manufacturing, Construction and Engineering", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 13, Values = "13", Name = "Media, Entertainment and Gaming", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 14, Values = "14", Name = "Retail", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 15, Values = "15", Name = "Telecommunications", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 16, Values = "16", Name = "Transport and Logistics", Indent = 0, IsSelected = false }
};

CompanySizes = new List<DataFilter>()
{
    new DataFilter() { Id = 1, Values = "1", Name = "Less than 10 employees", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 2, Values = "2", Name = "10 – 49 employees", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 3, Values = "3", Name = "50 – 249 employees", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 4, Values = "4", Name = "250 – 999 employees", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 5, Values = "5", Name = "1,000 – 4,999 employees", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 6, Values = "6", Name = "5,000 – 25,000 employees", Indent = 0, IsSelected = false },
    new DataFilter() { Id = 7, Values = "7", Name = "More than 25,000 employees", Indent = 0, IsSelected = false }
}; 
*/
