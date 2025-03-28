using System.Collections.Generic;

namespace DataDashboardSubscriptionsLib.Interfaces
{
    public interface ISubscriptionAccess
    {
        int CreateSubscription(string userId, string paymentReference, decimal amountPaid, string subscriptionReference, bool isGDPROptIn, bool isCommsOkay, int enquiryAllocation);
        List<Subscription> GetSubscriptionsForUser(string userId);
        void SetSubscriptionIsActive(int subscriptionId, bool isActive);
        void SetSubscriptionIsCommsOkay(int subscriptionId, bool isCommsOkay);
        void SetSubscriptionEnquiryAllocation(int subscriptionId, int enquiryAllocation);
        bool IsValidSubscription(string userId);
        bool SubscriptionIsGDPROptIn(string userId);
    }
}
