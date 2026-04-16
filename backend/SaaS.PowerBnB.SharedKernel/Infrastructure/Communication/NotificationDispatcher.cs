using Microsoft.Extensions.Logging;


namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Communication;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(ILogger<NotificationDispatcher> logger)
    {
        _logger = logger;
    }

    public async Task DispatchEmailAsync(string toAddress, string subject, string htmlBody)
    {
        _logger.LogInformation("Enviando E-mail via SendGrid para {ToAddress}", toAddress);
        await Task.CompletedTask;
    }

    public async Task DispatchSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("Enviando SMS via Twilio para {PhoneNumber}", phoneNumber);
        await Task.CompletedTask;
    }

    public async Task DispatchPushAsync(string deviceToken, string title, string body)
    {
        _logger.LogInformation("Enviando Push Notification via Firebase para {DeviceToken}", deviceToken);
        await Task.CompletedTask;
    }
}