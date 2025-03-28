namespace DataDashboardWebLib
{
    public class ReCaptchaResult
    {
        // Holds the result of reCAPTCHA validation
        public bool IsSuccess { get; set; } = false;
        public string ErrorMessage { get; set; }
    }

    public class UserResult
    {
        // Holds the result of a user creation
        public bool IsSuccess { get; set; } = false;
        public string UserId { get; set; }
        public string Password { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PaymentResult
    {
        // Holds the result of a payment
        public bool IsSuccess { get; set; } = false;
        public string PaymentReference { get; set; }
        public string ErrorMessage { get; set; }
        public int AmountPaid { get; set; }
        public string RedirectUrl { get; set; }
        public bool IsError { get; set; }
        public string ClientSecret { get; set; }
        public string SourceId { get; set; }
        public string Status { get; set; }
    }

    public class SubscriptionResult
    {
        // Holds the result of a subscription creation
        public bool IsSuccess { get; set; } = false;
        public int SubscriptionId { get; set; }
        public string ErrorMessage { get; set; }
        public string SubscriptionReference { get; set; }
    }

    public class TransactionResult
    {
        // Carries data relating to a complete transaction (user, payment, subscription)
        public bool IsComplete { get; set; } = false;
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string StripeToken { get; set; }
        public UserResult User { get; set; } = new UserResult();
        public PaymentResult Payment { get; set; } = new PaymentResult();
        public SubscriptionResult Subscription { get; set; } = new SubscriptionResult();
        public int SubscriptionPrice { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsCommsOkay { get; set; }
    }

    public class PaymentPostResult
    {
        public string ClientSecret { get; set; }
        public string EventType { get; set; }
        public bool IsSuccess { get; set; } = false;
    }
}
