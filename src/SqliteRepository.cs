using CaptureDataHost.Models;
using SqlKata.Execution;

namespace CaptureDataHost;

public class SqliteRepository : ISqliteRepository
{
    private string TABLE_USER_ACTION = "UserAction";
    private static string TABLE_COLUMNS = """
                                           
                                                       Id           TEXT  PRIMARY KEY,
                                                       Name         TEXT,
                                                       Image        TEXT NOT NULL,
                                                       Timestamp    INTEGER (15) NOT NULL
                                                   
                                           """;

    private readonly QueryFactory _db;

    public SqliteRepository(QueryFactory db)
    {
        _db = db;
        // Log the compiled query to the console
        _db.Logger = compiled =>
        {
            Console.WriteLine(compiled.ToString());
        };
        var query = $"CREATE TABLE IF NOT EXISTS {TABLE_USER_ACTION} ({TABLE_COLUMNS});";
        _db.Statement(query);
    }

    public Task DeleteAsync(string id)
    {
        return _db.Query(TABLE_USER_ACTION).Where("id", "=", id).DeleteAsync();
    }

    public Task HousekeepingAsync()
    {
        _db.Statement("PRAGMA wal_checkpoint(TRUNCATE)");
        _db.Statement("VACUUM");
        return Task.CompletedTask;
    }

    public async Task AddAsync(RecordedEntity entity)
    {
        var n = await _db.Query(TABLE_USER_ACTION).InsertAsync(new
        {
            Id = entity.Id,
            Name = entity.Name,
            Timestamp = entity.Timestamp,
            Image = entity.Image
        });
    }

    public Task<IEnumerable<RecordedEntity>> GetAllAsync()
    {
        return _db.Query(TABLE_USER_ACTION).OrderBy("Timestamp").GetAsync<RecordedEntity>();
    }

    public Task<RecordedEntity?> GetByIdAsync(string id)
    {
        return Task.FromResult(default(RecordedEntity));
    }
}