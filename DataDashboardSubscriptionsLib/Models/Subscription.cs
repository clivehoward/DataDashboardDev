using System;

namespace DataDashboardSubscriptionsLib
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public string UserId { get; set; }
        public string PaymentReference { get; set; }
        public string SubscriptionReference { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddYears(1);
        public bool IsActive { get; set; } = true;
        public bool IsGDPROptIn { get; set; } = false;
        public bool IsCommsOkay { get; set; } = false;
        public int EnquiryAllocation { get; set; }
    }
}
