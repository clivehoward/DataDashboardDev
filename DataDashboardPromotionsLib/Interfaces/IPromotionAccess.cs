namespace DataDashboardPromotionsLib.Interfaces
{
    public interface IPromotionAccess
    {
        bool AddSubmission(string firstName, string lastName, string email, string promotionName, bool isCommsOkay);
    }
}
