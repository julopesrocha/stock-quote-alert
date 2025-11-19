using System.Net;
using System.Net.Mail;
using Serilog;

/// Serviço responsável por enviar e-mails usando SMTP.
/// Usado para alertar quando o ativo ultrapassar limites configurados para compra/venda.

namespace StockQuoteAlert.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _destinationEmail;

        /// Construtor recebe todas as configurações necessárias para autenticação e envio de e-mails.

        public EmailService(string smtpHost, int smtpPort, string smtpUser, string smtpPassword, string destinationEmail)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPassword = smtpPassword;
            _destinationEmail = destinationEmail;
        }

        /// Envia um e-mail simples com título e corpo.
        public void SendEmail(string subject, string body)
        {
            try
            {
                var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
                };
                
                // Cria mensagem
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = body
                };

                mailMessage.To.Add(_destinationEmail);

                client.Send(mailMessage);
                Log.Information($">>> Email enviado: {subject}");
                Console.WriteLine($">>> Email enviado: {subject}");
            }
            catch (Exception ex)
            {
                // Pode falhar por autenticação, host incorreto, porta bloqueada etc.
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                Log.Error($"Erro ao enviar email: {ex.Message}");
            }
        }
    }
}
