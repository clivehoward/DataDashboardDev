using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;

namespace DataDashboardFunctions
{
    public static class SendEmailFunction
    {
        [FunctionName("SendEmail")]
        public static void Run([QueueTrigger("emailqueue", Connection = "AzureWebJobsStorage")] OutgoingEmail email, [SendGrid] out SendGridMessage message)
        {
            message = new SendGridMessage();
            message.AddTo(email.To);
            message.AddBcc("clivehoward@outlook.com");
            message.AddContent("text/html", email.Body);
            message.SetFrom(new EmailAddress(email.From));
            message.SetSubject(email.Subject);
        }
    }

    public class OutgoingEmail
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
