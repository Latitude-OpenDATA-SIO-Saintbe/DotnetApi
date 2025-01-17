using System.Data;
using Dapper;

namespace Metheo.DAL;

// Interface for DapperWrapper
public interface IDapperWrapper
{
    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object param = null);
}

// DapperWrapper implementation
public class DapperWrapper : IDapperWrapper
{
    public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object param = null)
    {
        return await connection.QueryAsync<T>(sql, param);
    }
}