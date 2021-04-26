using System.Threading.Tasks;


namespace WebToDoAPI.Utils
{
    public interface IEmailSender
    {
        Task SendPasswordToNewlyCreatedUserAsync(string userEmail, string userPassword);
        Task SendResetPasswordLinkToUserAsync(string userEmail, string resetPwdLink);
    }


    public class EmailSender : IEmailSender
    {
        public async Task SendPasswordToNewlyCreatedUserAsync(string userEmail, string userPassword)
        {
            System.Diagnostics.Debug.WriteLine($"send pwd to : [{userEmail}] pwd is : [{userPassword}]");
            //mock insted email sender 
            await Task.Delay(50);
        }

        public async Task SendResetPasswordLinkToUserAsync(string userEmail, string resetPwdLink)
        {
            System.Diagnostics.Debug.WriteLine($"send pwd reset token to : [{userEmail}] pwd reset token link is : [{resetPwdLink}]");
            //mock insted email sender 
            await Task.Delay(50);
        }
    }
}
