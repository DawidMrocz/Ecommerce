using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Common.Services.Database
{
    internal class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string not provided");
        }

        public async Task<IEnumerable<T>> ExecProcedure<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            return await ConnectionFrame(async connection => await connection.QueryAsync<T>(sql, AddParameters(parameters), commandType: CommandType.StoredProcedure, commandTimeout: timeout?.Seconds ?? TransactionManager.DefaultTimeout.Seconds), commandType, timeout, isolationLevel, scopeOption, parameters);
        }

        public virtual async Task<IEnumerable<T>> GetList<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            return await ConnectionFrame(async connection => await connection.QueryAsync<T>(sql, AddParameters(parameters), commandType: commandType ?? CommandType.Text, commandTimeout: timeout?.Seconds ?? TransactionManager.DefaultTimeout.Seconds), commandType, timeout, isolationLevel, scopeOption, parameters);
        }

        public virtual async Task<T?> Get<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            return await ConnectionFrame(async connection => (await connection.QueryAsync<T>(sql, AddParameters(parameters), commandType: commandType ?? CommandType.Text, commandTimeout: timeout?.Seconds ?? TransactionManager.DefaultTimeout.Seconds)).FirstOrDefault(), commandType, timeout, isolationLevel, scopeOption, parameters);
        }

        public virtual async Task<int> Create(string sql, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            return await ConnectionFrame(async connection => await connection.ExecuteAsync(sql, AddParameters(parameters), commandType: commandType ?? CommandType.Text, commandTimeout: timeout?.Seconds ?? TransactionManager.DefaultTimeout.Seconds), commandType, timeout, isolationLevel, scopeOption, parameters);
        }

        public virtual async Task<int> Update(string sql, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            return await ConnectionFrame(async connection => await connection.ExecuteAsync(sql, AddParameters(parameters), commandType: commandType ?? CommandType.Text, commandTimeout: timeout?.Seconds ?? TransactionManager.DefaultTimeout.Seconds), commandType, timeout, isolationLevel, scopeOption, parameters);
        }

        public virtual async Task<int> Delete(string sql, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            return await ConnectionFrame(async connection => await connection.ExecuteAsync(sql, AddParameters(parameters), commandType: commandType ?? CommandType.Text, commandTimeout: timeout?.Seconds ?? TransactionManager.DefaultTimeout.Seconds), commandType, timeout, isolationLevel, scopeOption, parameters);
        }

        public virtual async Task<T?> GetValue<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            return await ConnectionFrame(async connection => await connection.ExecuteScalarAsync<T>(sql, AddParameters(parameters), commandType: commandType ?? CommandType.Text, commandTimeout: timeout?.Seconds ?? TransactionManager.DefaultTimeout.Seconds), commandType, timeout, isolationLevel, scopeOption, parameters);
        }

        public TransactionScope GetTransactionScope(TransactionScopeOption? scopeOption = null, IsolationLevel? isolationLevel = null, TimeSpan? timeOut = null)
        {
            return new TransactionScope(scopeOption ?? TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = isolationLevel ?? IsolationLevel.ReadCommitted,
                Timeout = timeOut ?? TimeSpan.FromMinutes(1),
            }, TransactionScopeAsyncFlowOption.Enabled);
        }
        private static DynamicParameters AddParameters(params object[] parameters)
        {
            DynamicParameters _params = new();
            foreach (object parameter in parameters)
                _params.AddDynamicParams(parameter);

            return _params;
        }
        private async Task<T> ConnectionFrame<T>(Func<SqlConnection, Task<T>> operation, CommandType? commandType = null, TimeSpan? timeout = null, IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters)
        {
            using var transaction = GetTransactionScope(scopeOption, isolationLevel, timeout);
            using SqlConnection sqlConnection = new(_connectionString);
            sqlConnection.Open();
            var results = await operation(sqlConnection);
            transaction.Complete();
            return results;
        }
    }
}
