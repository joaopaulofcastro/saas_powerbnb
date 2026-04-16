using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;


public abstract class OutboxProcessorJobBase<TContext> : BackgroundService
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    protected OutboxProcessorJobBase(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // Pede ao injetor o banco de dados exato do módulo
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                // Precisamos garantir que o DbContext passado tenha a tabela OutboxMessages
                var messages = await dbContext.Set<OutboxMessage>()
                    .Where(m => m.ProcessedOnUtc == null)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        var domainEvent = JsonConvert.DeserializeObject<INotification>(
                            message.Content,
                            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
                        );

                        if (domainEvent is not null)
                        {
                            await publisher.Publish(domainEvent, stoppingToken);
                        }

                        message.MarkAsProcessed();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro no Outbox {MessageId} do contexto {Context}", message.Id, typeof(TContext).Name);
                        message.MarkAsFailed(ex.Message);
                    }
                }

                if (messages.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro fatal no Worker do Outbox para {Context}", typeof(TContext).Name);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}