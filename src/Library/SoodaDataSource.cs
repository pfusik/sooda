// 
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;

using Sooda.Schema;

namespace Sooda
{
    public abstract class SoodaDataSource
    {
        private string dataSourceName;

        protected SoodaDataSource(string dataSourceName)
        {
            this.dataSourceName = dataSourceName;
        }

        public string Name
        {
            get
            {
                return dataSourceName;
            }
        }

        protected string GetParameter(string name, bool throwOnFailure)
        {
            string val = SoodaConfig.GetString(this.dataSourceName + "." + name);
            if (val != null)
                return val;

            if (this.dataSourceName == "default")
            {
                val = SoodaConfig.GetString(name);
                if (val != null)
                    return val;
            }

            if (throwOnFailure)
                throw new SoodaException("Parameter " + name + " not defined for data source " + this.dataSourceName);
            return null;
        }

        public abstract void Rollback();
        public abstract void Commit();
        public abstract void Open();
        public abstract void Close();

        public abstract void SaveObjectChanges(SoodaObject obj);
        public abstract void DeleteObject(SoodaObject obj);
        public abstract IDataReader LoadObject(SoodaObject obj, object keyValue, out TableInfo[] tables);
        public abstract IDataReader LoadObjectTable(SoodaObject obj, object keyValue, int tableNumber, out TableInfo[] tables);
        public abstract void MakeTuple(string tableName, string leftColumn, string rightColumn, object leftVal, object rightVal, int mode);
        public abstract IDataReader LoadObjectList(ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, out TableInfo[] tables);
        public abstract IDataReader LoadRefObjectList(RelationInfo relationInfo, int masterColumn, object masterValue, out TableInfo[] tables);
        public abstract IDataReader ExecuteQuery(Sooda.QL.SoqlQueryExpression query, SchemaInfo schema, object[] parameters);
        public abstract IDataReader ExecuteRawQuery(string queryText, SchemaInfo schema, object[] parameters);

        public IDataReader ExecuteQuery(Sooda.QL.SoqlQueryExpression queryText, SchemaInfo schema, ArrayList parameters)
        {
            return ExecuteQuery(queryText, schema, parameters.ToArray());
        }
        public IDataReader ExecuteRawQuery(string queryText, SchemaInfo schema, ArrayList parameters)
        {
            return ExecuteRawQuery(queryText, schema, parameters.ToArray());
        }
    }
}
