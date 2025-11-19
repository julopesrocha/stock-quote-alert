using System.Net;
using System.Net.Mail;

namespace StockQuoteAlert.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _destinationEmail;

        public EmailService(string smtpHost, int smtpPort, string smtpUser, string smtpPassword, string destinationEmail)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPassword = smtpPassword;
            _destinationEmail = destinationEmail;
        }

        public void SendEmail(string subject, string body)
        {
            try
            {
                var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = body
                };

                mailMessage.To.Add(_destinationEmail);

                client.Send(mailMessage);
                Console.WriteLine($">>> Email enviado: {subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }
        }
    }
}
