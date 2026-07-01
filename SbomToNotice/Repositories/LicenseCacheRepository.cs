using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SbomToNotice.Repositories;

/// <summary>
/// Provides a simple local SQLite-backed cache for component license data.
/// </summary>
/// <param name="connectionString">The connection string.</param>
internal sealed class LicenseCacheRepository(string connectionString)
{
    /// <summary>
    /// The connection string.
    /// </summary>
    private readonly string _connectionString
        = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    /// <summary>
    /// A static dictionary that caches initialization (migration) tasks for each connection string
    /// </summary>
    private static readonly ConcurrentDictionary<string, Task> _initializerCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseCacheRepository"/> class. 
    /// </summary>
    public LicenseCacheRepository()
        : this(DataBases.LocalDbConnectionString)
    {
    }

    /// <summary>
    /// Migration will be performed automatically as needed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task EnsureDatabaseInitializedAsync() => _initializerCache.GetOrAdd(_connectionString, MigrateDatabaseAsync);

    /// <summary>
    /// Returns all migration definitions in the order they were defined (by version).
    /// </summary>
    /// <returns>The migration definitions.</returns>
    private static IEnumerable<(int Version, string Sql)> GetMigrations()
    {
        yield return (1, """
            CREATE TABLE IF NOT EXISTS LicenseCache(
                Name TEXT NOT NULL,
                Version TEXT NOT NULL,
                LicensesJson TEXT NOT NULL,
                PRIMARY KEY(
                    Name,
                    Version
                )
            );
            """);
    }

    /// <summary>
    /// Automated Migration Process Based on the Version Control Table Method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task MigrateDatabaseAsync(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        // If the migration history table does not exist, create it.
        const string createHistoryTableSql = """
            CREATE TABLE IF NOT EXISTS MigrationHistory(
                Version INTEGER NOT NULL PRIMARY KEY,
                AppliedAt TEXT NOT NULL
            );
            """;
        await connection.ExecuteAsync(createHistoryTableSql).ConfigureAwait(false);

        // Get the highest version that has been applied.
        const string getLatestVersionSql = """
            SELECT IFNULL(MAX(Version), 0)
            FROM MigrationHistory;
            """;
        var currentVersion = await connection.ExecuteScalarAsync<int>(getLatestVersionSql).ConfigureAwait(false);

        // Run the pending migrations in order.
        foreach (var migration in GetMigrations()
            .Where(m => m.Version > currentVersion)
            .OrderBy(m => m.Version))
        {
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                // Run the migration.
                await connection.ExecuteAsync(migration.Sql, transaction: transaction).ConfigureAwait(false);

                // Record in the history table.
                const string insertHistorySql = """
                    INSERT INTO MigrationHistory(
                        Version,
                        AppliedAt
                    )
                    VALUES(
                        @Version,
                        @AppliedAt
                    );
                    """;
                await connection.ExecuteAsync(insertHistorySql, new
                {
                    migration.Version,
                    AppliedAt = DateTime.UtcNow.ToString("o")
                }, transaction: transaction).ConfigureAwait(false);

                // Commit.
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                // Rollback.
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }

    /// <summary>
    /// Retrieve the license information from the cache.
    /// </summary>
    /// <returns>The list of the license text.</returns>
    public async Task<IReadOnlyList<string>?> GetLicensesAsync(string name, string version)
    {
        await EnsureDatabaseInitializedAsync().ConfigureAwait(false);

        const string sql = """
            SELECT LicensesJson
            FROM LicenseCache
            WHERE Name = @Name
                AND VERSION = @Version;
            """;

        using var connection = new SqliteConnection(_connectionString);
        var json = await connection.QueryFirstOrDefaultAsync<string>(sql,
            new
            {
                Name = name,
                Version = version
            }).ConfigureAwait(false);

        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<List<string>>(json);
    }

    /// <summary>
    /// Saves the license information to the cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveLicensesAsync(string name, string version, IEnumerable<string> licenses)
    {
        await EnsureDatabaseInitializedAsync().ConfigureAwait(false);

        const string sql = """
            INSERT INTO LicenseCache(
                Name,
                Version,
                LicensesJson
            )
            VALUES(
                @Name,
                @Version,
                @LicensesJson
            )
            ON CONFLICT(
                    Name,
                    Version
                )
            DO UPDATE
                SET LicensesJson = excluded.LicensesJson
            ;
            """;

        var json = JsonSerializer.Serialize(licenses);

        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(sql,
            new
            {
                Name = name,
                Version = version,
                LicensesJson = json
            }).ConfigureAwait(false);
    }
}
