namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Communication;

public interface INotificationDispatcher
{
    Task DispatchEmailAsync(string toAddress, string subject, string htmlBody);
}
