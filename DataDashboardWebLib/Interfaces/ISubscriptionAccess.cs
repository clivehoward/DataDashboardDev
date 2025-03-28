using System.Threading.Tasks;

namespace DataDashboardWebLib.Interfaces
{
    public interface ISubscriptionAccess
    {
        Task<int> CreateSubscription(string userId, string paymentReference, decimal amountPaid, string subscriptionReference, bool isGDPROptIn, bool isCommsOkay, int enquiryAllocation);
        Task<bool> SetSubscriptionIsActive(int subscriptionId, bool isActive);
        Task<bool> SetSubscriptionIsCommsOkay(int subscriptionId, bool isCommsOkay);
        Task<Subscription> GetSubscription(string userId);
        Task<bool> IsValidSubscription(string userId);
        Task<bool> IsGDPROptIn(string userId);
        Task<bool> SetSubscriptionEnquiryAllocation(int subscriptionId, int enquiryAllocation);
    }
}
