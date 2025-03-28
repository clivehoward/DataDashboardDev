using System.Threading.Tasks;

namespace DataDashboardMessagingLib.Interfaces
{
    public interface IEmailAccess
    {
        OutgoingEmail CreateOutgoingEmail(string to, string from, string subject, string body);
        string CreateMessageBody(string templatePath, params string[] args);
        Task Send(OutgoingEmail email);
    }
}
