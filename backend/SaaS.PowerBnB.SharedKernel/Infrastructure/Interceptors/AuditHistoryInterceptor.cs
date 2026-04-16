using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SaaS.PowerBnB.SharedKernel.Application.Interfaces;
using SaaS.PowerBnB.SharedKernel.Audit;
using SaaS.PowerBnB.SharedKernel.Domain;



namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Interceptors;


public class AuditHistoryInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    // Opções para o JSON ficar limpo no banco
    private static readonly JsonSerializerSettings _jsonOptions = new()
    {
        FloatFormatHandling = FloatFormatHandling.String,
        FloatParseHandling = FloatParseHandling.Double,

        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy() // Mantém o padrão do banco
        },

        Formatting = Formatting.None, // Compacto para o banco de dados
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    public AuditHistoryInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var userId = _currentUserService.UserId ?? "System";
        var auditEntries = new List<AuditLog>();

        // Filtramos APENAS entidades que herdam de EntityBase
        // Isso evita auditar o próprio AuditLog ou tabelas de Outbox
        var entries = context.ChangeTracker.Entries<EntityBase>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var tableName = entry.Metadata.ClrType.Name;
            var recordId = entry.Entity.Id;
            var action = entry.State.ToString();

            string? oldValues = null;
            string? newValues = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues = SerializeValues(entry.CurrentValues);
                    break;

                case EntityState.Deleted:
                    oldValues = SerializeValues(entry.OriginalValues);
                    break;

                case EntityState.Modified:
                    // Pega o estado antes e depois da alteração
                    oldValues = SerializeValues(entry.OriginalValues);
                    newValues = SerializeValues(entry.CurrentValues);
                    break;
            }

            auditEntries.Add(new AuditLog(tableName, recordId, action, oldValues, newValues, userId));
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Método privado para converter os valores complexos do EF Core em um Dicionário,
    /// permitindo que o System.Text.Json gere a string corretamente.
    /// </summary>
    private string SerializeValues(PropertyValues values)
    {
        var dictionary = new Dictionary<string, object?>();

        foreach (var property in values.Properties)
        {
            // Pega o valor da coluna específica
            var value = values[property];
            dictionary[property.Name] = value;
        }

        return JsonConvert.SerializeObject(dictionary, _jsonOptions);
    }
}