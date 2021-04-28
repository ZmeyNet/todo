using System.Threading.Tasks;


namespace WebToDoAPI.Utils
{
    public class EmailSender : IEmailSender
    {
        public async Task SendPasswordToNewlyCreatedUserAsync(string userEmail, string userPassword)
        {
            System.Diagnostics.Debug.WriteLine($"send pwd to : [{userEmail}] pwd is : [{userPassword}]");
            //mock instead email sender 
            await Task.CompletedTask;
        }

        public async Task SendResetPasswordLinkToUserAsync(string userEmail, string resetPwdLink)
        {
            System.Diagnostics.Debug.WriteLine($"send pwd reset token to : [{userEmail}] pwd reset token link is : [{resetPwdLink}]");
            //mock instead email sender 
            await Task.CompletedTask;
        }
    }
}
