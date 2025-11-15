using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace ECinema.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mohamedhamada14512@gmail.com", "cusf qmzi ccbm wopm")
            };

            return client.SendMailAsync(
            new MailMessage(from: "mohamedhamada14512@gmail.com",
                            to: email,
                            subject,
                            htmlMessage
                            )
            {
                IsBodyHtml = true
            });
        }
    }
}
