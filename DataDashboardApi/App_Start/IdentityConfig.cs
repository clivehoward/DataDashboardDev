using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using DataDashboardApi.Models;
using System.Configuration;
using System.IO;
using System.Web;
using DataDashboardMessagingLib;

namespace DataDashboardApi
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            manager.EmailService = new EmailService(); //  Adds the new email service that we have created below
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            // Get Azure Storage Connection key from web.config
            string azureStorageConnection = ConfigurationManager.AppSettings["AzureStorageConnection"];

            // Now we send the email (which actually means adding to a Storage Queue to be sent by an Azure Function)
            EmailAccess emailAccess = new EmailAccess(azureStorageConnection);

            // Get our template
            // Path to the email template
            string webRootPath = HttpContext.Current.Server.MapPath("/Templates");
            string templatePath = webRootPath + Path.DirectorySeparatorChar.ToString()
                                        + "Email" + Path.DirectorySeparatorChar.ToString()
                                        + "ForgottenPassword.html";

            // Use it to create the email body text 
            // NOTE: In our controller method we set the To email (Destination), Subject and Body...
            // But, we set the body to the password reset token ONLY, the rest of the body copy is in our template
            // Odd but okay !!
            string body = emailAccess.CreateMessageBody(templatePath, message.Body);

            // Now we create the outgoing email object
            OutgoingEmail email = emailAccess.CreateOutgoingEmail(message.Destination, "no-reply@discoveredinsights.com", message.Subject, body);

            // And send it...
            await emailAccess.Send(email);
        }
    }

    // NEW: Added this in order to include Roles in the Account controller
    // This then gets injected via StartupAuth.cs into AccountController.cs
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> store)
            : base(store)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var manager = new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));
            return manager;
        }
    }
}
