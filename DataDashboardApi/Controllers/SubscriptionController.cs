using DataDashboardApi.Models;
using DataDashboardSubscriptionsLib;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace DataDashboardApi.Controllers
{
    public class SubscriptionController : ApiController
    {
        // Create Subscription
        [Route("subscription")]
        [HttpPost]
        public IHttpActionResult Post(CreateSubscriptionBindingModel model)
        {
            string userId = model.UserId;
            string paymentReference = model.PaymentReference;
            decimal amountPaid = model.AmountPaid;
            string subscriptionReference = model.SubscriptionReference;
            bool isGDPROptIn = model.IsGDPROptIn;
            bool isCommsOkay = model.IsCommsOkay;
            int enquiryAllocation = model.EnquiryAllocation;

            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            SubscriptionAccess subscription = new SubscriptionAccess(dbconn);

            // Create a new subscription and get the ID
            int id = subscription.CreateSubscription(userId, paymentReference, amountPaid, subscriptionReference, isGDPROptIn, isCommsOkay, enquiryAllocation);

            if (id <= 0)
            {
                return BadRequest("Subscription could not be created");
            }

            // Success and return the new id
            return Ok(id);
        }

        // Change IsActivate
        [Route("subscription/{id:int:min(1)}")]
        [HttpPut]
        public IHttpActionResult Put(int id, ChangeSubscriptionIsActiveModel model)
        {
            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            SubscriptionAccess subscription = new SubscriptionAccess(dbconn);

            // Change the setting on the subscription
            subscription.SetSubscriptionIsActive(id, model.IsActive);

            // Return only the first one
            return Ok();
        }

        // Change IsCommsOkay
        [Route("subscription/comms/{id:int:min(1)}")]
        [HttpPut]
        public IHttpActionResult Put(int id, ChangeSubscriptionIsCommsOkayModel model)
        {
            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            SubscriptionAccess subscription = new SubscriptionAccess(dbconn);

            // Change the setting on the subscription
            subscription.SetSubscriptionIsCommsOkay(id, model.IsCommsOkay);

            // Return only the first one
            return Ok();
        }

        // Return user's latest subscriptions
        [Authorize]
        [Route("subscription")]
        [HttpGet]
        public IHttpActionResult Get(string userId)
        {
            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            SubscriptionAccess subscription = new SubscriptionAccess(dbconn);

            // Get all of the users subscriptions
            List<Subscription> subscriptions = subscription.GetSubscriptionsForUser(userId);

            // Return only the first one
            return Ok(subscriptions.FirstOrDefault());
        }

        // Return whether the user has a valid subscription
        [Authorize]
        [Route("subscription/valid")]
        [HttpGet]
        public IHttpActionResult IsValid(string userId)
        {
            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            SubscriptionAccess subscription = new SubscriptionAccess(dbconn);

            // Get whether there is a valid subscription
            bool isValid = subscription.IsValidSubscription(userId);

            // Return only the first one
            return Ok(isValid);
        }

        // Return whether the user has actively complied with GDPR
        [Authorize]
        [Route("subscription/gdpr")]
        [HttpGet]
        public IHttpActionResult IsGDPROptIn(string userId)
        {
            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            SubscriptionAccess subscription = new SubscriptionAccess(dbconn);

            // Get whether the user has actively complied with GDPR
            bool isGDPROptIn = subscription.SubscriptionIsGDPROptIn(userId);

            // Return only the first one
            return Ok(isGDPROptIn);
        }

        // Change EnquiryAllocation
        [Route("subscription/enquiry/{id:int:min(1)}")]
        [HttpPut]
        public IHttpActionResult Put(int id, ChangeSubscriptionEnquiryAllocation model)
        {
            // Get DB connection string from properties
            string dbconn = ConfigurationManager.ConnectionStrings["dbDataDashboardConnString"].ConnectionString;

            SubscriptionAccess subscription = new SubscriptionAccess(dbconn);

            // Change the setting on the subscription
            subscription.SetSubscriptionEnquiryAllocation(id, model.EnquiryAllocation);

            return Ok();
        }
    }
}
