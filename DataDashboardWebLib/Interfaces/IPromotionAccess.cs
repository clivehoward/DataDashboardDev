using System.Threading.Tasks;

namespace DataDashboardWebLib.Interfaces
{
    public interface IPromotionAccess
    {
        Task<bool> AddSubmission(string firstName, string lastName, string email, string promotionName, bool isCommsOkay);

    }
}
