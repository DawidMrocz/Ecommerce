using System.Data;
using System.Transactions;

namespace Common.Services.Database
{
    public interface IDatabaseService
    {
        Task<int> Create(string sql, CommandType? commandType = null, TimeSpan? timeout = null, System.Transactions.IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters);
        Task<int> Delete(string sql, CommandType? commandType = null, TimeSpan? timeout = null, System.Transactions.IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters);
        Task<IEnumerable<T>> ExecProcedure<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, System.Transactions.IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters);
        Task<T?> Get<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, System.Transactions.IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters);
        Task<IEnumerable<T>> GetList<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, System.Transactions.IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters);
        TransactionScope GetTransactionScope(TransactionScopeOption? scopeOption = null, System.Transactions.IsolationLevel? isolationLevel = null, TimeSpan? timeOut = null);
        Task<T?> GetValue<T>(string sql, CommandType? commandType = null, TimeSpan? timeout = null, System.Transactions.IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters);
        Task<int> Update(string sql, CommandType? commandType = null, TimeSpan? timeout = null, System.Transactions.IsolationLevel? isolationLevel = null, TransactionScopeOption? scopeOption = null, params object[] parameters);
    }
}
