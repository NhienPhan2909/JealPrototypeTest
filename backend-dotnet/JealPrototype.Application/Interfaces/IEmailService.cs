namespace JealPrototype.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendLeadNotificationAsync(string dealershipEmail, string leadName, string leadEmail, string leadPhone, string message, string? vehicleTitle = null, CancellationToken cancellationToken = default);
    Task SendSalesRequestNotificationAsync(string dealershipEmail, string name, string email, string phone, string make, string model, int year, int kilometers, string? message = null, CancellationToken cancellationToken = default);
}
