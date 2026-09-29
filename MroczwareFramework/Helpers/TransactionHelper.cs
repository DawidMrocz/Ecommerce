using System;
using System.Transactions;

namespace MroczwareFramework.Helpers
{
    public static class TransactionHelper
    {
        public static TransactionScope GetTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(30.0)
            }, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
