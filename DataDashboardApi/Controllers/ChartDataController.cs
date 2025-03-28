using DataDashboardInsightLib;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace DataDashboardApi.Controllers
{
    [Authorize]
    public class ChartDataController : ApiController
    {
        [Route("question/{id:int:min(1)}")]
        [HttpGet]
        public IHttpActionResult GetChartData(int id, string Location = "0", string CompanySize = "0", string Industry = "0")
        {
            // Input parameters must be legitimate
            if (System.Text.RegularExpressions.Regex.IsMatch(Location, "^([0-9]+,)*[0-9]+$") == false)
            {
                return BadRequest("Location must be a single integer or a comma separate list of integers");
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(CompanySize, "^([0-9]+,)*[0-9]+$") == false)
            {
                return BadRequest("CompanySize must be a single integer or a comma separate list of integers");
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(Industry, "^([0-9]+,)*[0-9]+$") == false)
            {
                return BadRequest("Industry must be a single integer or a comma separate list of integers");
            }

            // Get the database connection string from settings
            string dbConnString = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;
                //Properties.Settings.Default.dbDataDashboardConnString;

            // Create an instance of the DataAccess class and pass the connection string
            DataAccess dataAccess = new DataAccess(dbConnString);

            // Get the ChartData object for the specified question
            ChartData chartData = dataAccess.GetChartDataForQuestion(questionId: id, locationId: Location, companySizeId: CompanySize, industryId: Industry);

            //if (id == null)
            //{
            //    return NotFound();
            //}

            return Ok(chartData);
        }

        [Route("coverage")]
        [HttpGet]
        public IHttpActionResult GetCoverage(string Location = "0", string CompanySize = "0", string Industry = "0")
        {
            // Input parameters must be legitimate
            if (System.Text.RegularExpressions.Regex.IsMatch(Location, "^([0-9]+,)*[0-9]+$") == false)
            {
                return BadRequest("Location must be a single integer or a comma separate list of integers");
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(CompanySize, "^([0-9]+,)*[0-9]+$") == false)
            {
                return BadRequest("CompanySize must be a single integer or a comma separate list of integers");
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(Industry, "^([0-9]+,)*[0-9]+$") == false)
            {
                return BadRequest("Industry must be a single integer or a comma separate list of integers");
            }

            // Get the database connection string from settings
            string dbConnString = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            // Create an instance of the DataAccess class and pass the connection string
            DataAccess dataAccess = new DataAccess(dbConnString);

            // Get the coverage results for the given filters
            List<CoverageResult> coverage = dataAccess.GetDataCoverage(locationId: Location, companySizeId: CompanySize, industryId: Industry);

            return Ok(coverage);
        }
    }
}
