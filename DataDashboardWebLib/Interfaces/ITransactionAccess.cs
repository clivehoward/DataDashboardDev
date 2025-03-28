using System.Threading.Tasks;

namespace DataDashboardWebLib.Interfaces
{
    public interface ITransactionAccess
    {
        Task<ReCaptchaResult> ValidateReCaptcha(string reCaptchaKey, string response, string remoteip);
        Task<UserResult> CreateNewUser(string email, string firstName, string lastName, string roleName);
        Task<SubscriptionResult> CreateNewSubscription(string userId, string paymentReference, decimal amount, bool isGDPROptIn, bool isCommsOkay, int enquiryAllocation);
    }
}
