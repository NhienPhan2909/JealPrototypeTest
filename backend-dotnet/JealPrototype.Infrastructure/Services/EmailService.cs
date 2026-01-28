using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using JealPrototype.Application.Interfaces;

namespace JealPrototype.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["EmailSettings:FromName"] ?? "Jeal Prototype",
            _configuration["EmailSettings:Username"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["EmailSettings:SmtpHost"],
            int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"),
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            _configuration["EmailSettings:Username"],
            _configuration["EmailSettings:Password"],
            cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendLeadNotificationAsync(
        string dealershipEmail,
        string leadName,
        string leadEmail,
        string leadPhone,
        string message,
        string? vehicleTitle = null,
        CancellationToken cancellationToken = default)
    {
        var subject = vehicleTitle != null 
            ? $"New Lead Enquiry for {vehicleTitle}" 
            : "New General Enquiry";

        var htmlBody = $@"
            <html>
            <body>
                <h2>New Lead Received</h2>
                <p><strong>Name:</strong> {leadName}</p>
                <p><strong>Email:</strong> {leadEmail}</p>
                <p><strong>Phone:</strong> {leadPhone}</p>
                {(vehicleTitle != null ? $"<p><strong>Vehicle:</strong> {vehicleTitle}</p>" : "")}
                <p><strong>Message:</strong></p>
                <p>{message}</p>
            </body>
            </html>";

        await SendEmailAsync(dealershipEmail, subject, htmlBody, cancellationToken);
    }

    public async Task SendSalesRequestNotificationAsync(
        string dealershipEmail,
        string name,
        string email,
        string phone,
        string make,
        string model,
        int year,
        int kilometers,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        var subject = "New 'Sell Your Car' Request";

        var htmlBody = $@"
            <html>
            <body>
                <h2>New Sell Your Car Request</h2>
                <h3>Customer Information</h3>
                <p><strong>Name:</strong> {name}</p>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Phone:</strong> {phone}</p>
                <h3>Vehicle Information</h3>
                <p><strong>Make:</strong> {make}</p>
                <p><strong>Model:</strong> {model}</p>
                <p><strong>Year:</strong> {year}</p>
                <p><strong>Kilometers:</strong> {kilometers:N0}</p>
                {(!string.IsNullOrWhiteSpace(message) ? $"<p><strong>Additional Message:</strong><br/>{message}</p>" : "")}
            </body>
            </html>";

        await SendEmailAsync(dealershipEmail, subject, htmlBody, cancellationToken);
    }
}
