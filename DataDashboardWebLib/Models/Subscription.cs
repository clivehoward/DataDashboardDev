using System;

namespace DataDashboardWebLib
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public string UserId { get; set; }
        public string PaymentReference { get; set; }
        public string SubscriptionReference { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsGDPROptIn { get; set; } = false;
        public bool IsCommsOkay { get; set; } = false;
        public int EnquiryAllocation { get; set; }
    }
}
