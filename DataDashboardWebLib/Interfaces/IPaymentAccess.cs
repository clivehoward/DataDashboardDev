namespace DataDashboardWebLib.Interfaces
{
    public interface IPaymentAccess
    {
        PaymentResult Make3DSecurePayment(decimal price, string stripeToken, string returnUrl);
        PaymentResult MakePayment(decimal price, string stripeToken, string productDescription);
        PaymentPostResult GetPaymentPostResult(string json, string signature, string secret);
    }
}
