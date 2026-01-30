using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Kemet_api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPass = _configuration["EmailSettings:SmtpPass"]?.Replace(" ", ""); // Sanitize password
            var fromEmail = _configuration["EmailSettings:FromEmail"];

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(fromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            try
            {
                // SecureSocketOptions.StartTls is for port 587
                // If using port 465, use SecureSocketOptions.SslOnConnect
                var options = smtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                
                await smtp.ConnectAsync(smtpHost, smtpPort, options);
                await smtp.AuthenticateAsync(smtpUser, smtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log the specific error for debugging
                throw new Exception($"Failed to send email. Host: {smtpHost}:{smtpPort}, User: {smtpUser}. Error: {ex.Message}", ex);
            }
        }

        public async Task SendOtpEmailAsync(string to, string otp)
        {
            var subject = "Your Verification Code";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #4CAF50; text-align: center;'>Welcome to Kemet!</h2>
                    <p>Hello,</p>
                    <p>Thank you for choosing Kemet. Use the following dynamic verification code to complete your process:</p>
                    <div style='background-color: #f9f9f9; padding: 15px; text-align: center; border-radius: 5px; margin: 20px 0;'>
                        <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #333;'>{otp}</span>
                    </div>
                    <p>This code will expire in 10 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777; text-align: center;'>&copy; 2026 Kemet Project. All rights reserved.</p>
                </div>";
            
            await SendEmailAsync(to, subject, body);
        }
    }
}
