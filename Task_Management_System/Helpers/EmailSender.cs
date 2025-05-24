using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Task_Management_System.Helpers
{
    public class EmailSender
    {
        private readonly string _smtpHost = "smtp.ethereal.email"; 
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "koby.jerde76@ethereal.email";
        private readonly string _smtpPass = "1RZMgEvwnaUEDkPhwG";

        public async Task SendEmailAsync(string toEmail, string subject, string message, string fromName = null)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_smtpUser, fromName ?? "Task Management System"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }
    }
}
