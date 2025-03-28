using DataDashboardApi.Models;
using DataDashboardPromotionsLib;
using System.Configuration;
using System.Web.Http;

namespace DataDashboardApi.Controllers
{
    public class PromotionsController : ApiController
    {
        [Route("promotion/submission")]
        [HttpPost]
        public IHttpActionResult Post(AddSubmissionBindingModel model)
        {
            string firstName = model.FirstName;
            string lastName = model.LastName;
            string email = model.Email;
            string promotionName = model.PromotionName;
            bool isCommsOkay = model.IsCommsOkay;

            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            PromotionAccess promotionAccess = new PromotionAccess(dbconn);

            // Add the submission
            bool okay = promotionAccess.AddSubmission(firstName, lastName, email, promotionName, isCommsOkay);

            if (okay == false)
            {
                return BadRequest("Submission could not be added");
            }

            return Ok();
        }
    }
}
