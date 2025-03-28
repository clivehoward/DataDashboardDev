using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataDashboardWebLib.Interfaces
{
    public interface IUserAccess
    {
        Task<string> RegisterUser(string email, string password, string firstName, string lastName);
        Task<string> GetUserId(string email);
        Task<UserInfo> GetUser();
        Task<bool> ChangePassword(string oldPassword, string newPassword);
        Task<bool> UpdateUser(string email, string firstName, string lastName);
        void Logout();
        Task Delete();
        Task Delete(string userId);
        Task AddToRole(string userId, string roleName);
        Task<List<string>> GetUserRoles(string bearerToken);
        Task<string> GetBearerToken(string email, string password);
        Task<bool> ForgotPassword(string email);
        Task<bool> ResetPassword(string email, string code, string password);
    }
}
