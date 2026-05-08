using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.Modules.Financial.Domain.Entities;
using SaaS.PowerBnB.Modules.Financial.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Communication.Events;

namespace SaaS.PowerBnB.Modules.Financial.Features.UserRegistration;

internal class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly FinancialDbContext _dbContext;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        FinancialDbContext dbContext,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Idempotência: verifica se o FinancialUser já existe
        var exists = await _dbContext.FinancialUsers
            .AnyAsync(u => u.Id == notification.UserId, cancellationToken);

        if (exists)
        {
            _logger.LogInformation(
                "FinancialUser {UserId} já existe. Evento ignorado (idempotência).",
                notification.UserId);
            return;
        }

        var financialUser = new FinancialUser(
            notification.UserId,
            notification.Name,
            notification.Email);

        await _dbContext.FinancialUsers.AddAsync(financialUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "FinancialUser criado com sucesso. UserId: {UserId}",
            notification.UserId);
    }
}
