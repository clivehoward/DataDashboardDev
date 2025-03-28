using DataDashboardPromotionsLib.Interfaces;

namespace DataDashboardPromotionsLib
{
    public class PromotionAccess : IPromotionAccess
    {
        private string DbConnectionString;

        public PromotionAccess(string dbConnectionString)
        {
            // We pass in the connection string when an instance is instantiated
            DbConnectionString = dbConnectionString;
        }

        public bool AddSubmission(string firstName, string lastName, string email, string promotionName, bool isCommsOkay)
        {
            // Add a new Submission
            using (var db = new PromotionContext(DbConnectionString))
            {
                var submission = new Submission
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PromotionName = promotionName,
                    IsGDPROptIn = true,
                    IsCommsOkay = isCommsOkay
                };

                db.Submissions.Add(submission);
                db.SaveChanges();
            }

            return true;
        }
    }
}
