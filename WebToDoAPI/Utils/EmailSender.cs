using System.Threading.Tasks;


namespace WebToDoAPI.Utils
{
    public interface IEmailSender
    {
        Task SendPasswordToNewlyCreatedUserAsync(string userEmail, string userPassword);
    }


    public class EmailSender : IEmailSender
    {
        public async Task SendPasswordToNewlyCreatedUserAsync(string userEmail, string userPassword)
        {
            System.Diagnostics.Debug.WriteLine($"send pwd to : [{userEmail}] pwd is : [{userPassword}]");
            //mock insted email sender 
            await Task.Delay(50);
        }
    }
}
