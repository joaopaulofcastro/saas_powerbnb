using System.Data;
using System.Diagnostics;
using Dapper;
using Npgsql;

namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

/// <summary>
/// Executor de SQL de alta performance com telemetria integrada.
/// Utiliza Dapper + Npgsql para consultas de leitura (Query side do CQRS)
/// e operações em lote (ex: Outbox processor).
/// Cada execução gera um span no Jaeger via OpenTelemetry.
/// </summary>
public sealed class SqlExecutor
{
    private readonly string _connectionString;

    // ActivitySource para que cada execução de SQL apareça como span no Jaeger
    private static readonly ActivitySource _activitySource = new("SaaS.PowerBnB.SqlExecutor");

    public SqlExecutor(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Executa um comando SQL (INSERT, UPDATE, DELETE) e retorna o número de linhas afetadas.
    /// Suporta transações externas para uso dentro do padrão Unit of Work.
    /// </summary>
    public async Task<int> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"SQL {GetOperationName(sql)}", ActivityKind.Client);
        activity?.SetTag("db.system", "postgresql");
        activity?.SetTag("db.statement", sql);

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var commandDefinition = new CommandDefinition(
                sql,
                param,
                transaction,
                cancellationToken: cancellationToken);

            var result = await connection.ExecuteAsync(commandDefinition);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Executa uma query SELECT e retorna uma lista tipada de resultados.
    /// Ideal para o Query side do CQRS — leitura direta sem passar pelo EF Core.
    /// </summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? param = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Query {typeof(T).Name}", ActivityKind.Client);
        activity?.SetTag("db.system", "postgresql");
        activity?.SetTag("db.statement", sql);

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var commandDefinition = new CommandDefinition(
                sql,
                param,
                cancellationToken: cancellationToken);

            var result = await connection.QueryAsync<T>(commandDefinition);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Executa uma query SELECT e retorna um único resultado ou null.
    /// Lança exceção se mais de um registro for encontrado.
    /// </summary>
    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? param = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"QuerySingle {typeof(T).Name}", ActivityKind.Client);
        activity?.SetTag("db.system", "postgresql");
        activity?.SetTag("db.statement", sql);

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var commandDefinition = new CommandDefinition(
                sql,
                param,
                cancellationToken: cancellationToken);

            var result = await connection.QuerySingleOrDefaultAsync<T>(commandDefinition);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    // Extrai a primeira palavra do SQL (SELECT, INSERT, UPDATE, DELETE)
    // para nomear o span no Jaeger de forma legível.
    private static string GetOperationName(string sql)
    {
        var trimmed = sql.AsSpan().TrimStart();
        var firstSpace = trimmed.IndexOf(' ');
        return firstSpace > 0
            ? trimmed[..firstSpace].ToString().ToUpperInvariant()
            : "EXECUTE";
    }
}
