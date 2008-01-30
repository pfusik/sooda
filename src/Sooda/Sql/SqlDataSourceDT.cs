// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 
#if DOTNET2

using System;
using System.Text;
using System.Data;
using System.IO;
using System.Collections;

using Sooda.Schema;
using Sooda.QL;
using Sooda.Utils;

using Sooda.Logging;

namespace Sooda.Sql
{
    /// <summary>
    /// Data source that allows external (DTC) transactions
    /// How? By not using ADO.Net transaction when there is active external transaction.
    /// No changes other from that.
    /// </summary>
    public class SqlDataSourceDT : SqlDataSource
    {
        
        public SqlDataSourceDT(string name) : base(name)
        {
        }

        public SqlDataSourceDT(Sooda.Schema.DataSourceInfo dataSourceInfo) : this(dataSourceInfo.Name)
        {
        }


        private bool InExternalTransaction()
        {
            if (System.Transactions.Transaction.Current != null)
            {
				//TransactionStatus ts = System.Transactions.Transaction.Current.TransactionInformation.Status;
				//if (ts == System.Transactions.TransactionStatus.Active || ts == System.Transactions.TransactionStatus.InDoubt)
				return true;
            }
            return false;
        }

        private void BeginTransactionIfNecessary()
        {
            if (Transaction != null)
                logger.Warn("Previous transaction has not been closed");
            if (!DisableTransactions)
            {
                if (InExternalTransaction())
                {
                    logger.Debug("External transaction exists, will not start ADO transaction");
                }
                else
                {
                    logger.Debug("Starting new ADO.Net transaction");
                    Transaction = Connection.BeginTransaction(IsolationLevel);
                }
            }

        }

        public override void Open()
        {
            if (ConnectionString == null)
                throw new SoodaDatabaseException("connectionString parameter not defined for datasource: " + Name);
            if (ConnectionType == null)
                throw new SoodaDatabaseException("connectionType parameter not defined for datasource: " + Name);
            Connection = (IDbConnection)Activator.CreateInstance(ConnectionType, new object[] { ConnectionString });
            Connection.Open();
            BeginTransactionIfNecessary();
        }

        public override void Rollback()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = null;
            }
            BeginTransactionIfNecessary();
        }

        public override void Commit()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;
            }
            BeginTransactionIfNecessary();
        }

        public override void Close()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = null;
            };

            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }
    }
}

#endif