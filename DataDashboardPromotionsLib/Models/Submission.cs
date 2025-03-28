using System;

namespace DataDashboardPromotionsLib
{
    class Submission
    {
        public int SubmissionId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PromotionName { get; set; }
        public bool IsGDPROptIn { get; set; } = false;
        public bool IsCommsOkay { get; set; } = false;
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
    }
}
