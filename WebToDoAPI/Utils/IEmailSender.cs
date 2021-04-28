using System.Threading.Tasks;

namespace WebToDoAPI.Utils
{
    public interface IEmailSender
    {
        Task SendPasswordToNewlyCreatedUserAsync(string userEmail, string userPassword);
        Task SendResetPasswordLinkToUserAsync(string userEmail, string resetPwdLink);
    }
}