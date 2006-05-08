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

using System.Data;

using Sooda.Schema;

namespace Sooda.Xml
{
    public class XmlDataSource : Sooda.SoodaDataSource
    {
        DataSet ds = new DataSet();

        public XmlDataSource(DataSourceInfo dataSourceInfo)
            : base(dataSourceInfo)
        {
            // ds.Relations.Add(new DataRelation(
        }

        public override void Open() { }

        public override void Rollback() { }

        public override void Commit() { }

        public override void Close() { }

        public override IDataReader LoadObject(Sooda.SoodaObject obj, object keyValue, out TableInfo[] tables)
        {
            tables = null;
            // TODO:  Add XmlDataSource.LoadObject implementation
            return null;
        }

        public override IDataReader LoadMatchingPrimaryKeys(SchemaInfo schemaInfo, ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount)
        {
            return null;
        }

        public override IDataReader LoadObjectList(SchemaInfo schema, Sooda.Schema.ClassInfo classInfo, Sooda.SoodaWhereClause whereClause, Sooda.SoodaOrderBy orderBy, int topCount, SoodaSnapshotOptions options, out TableInfo[] tables)
        {
            tables = null;
            // TODO:  Add XmlDataSource.LoadObjectList implementation
            return null;
        }

        public override IDataReader LoadRefObjectList(SchemaInfo schema, Sooda.Schema.RelationInfo relationInfo, int masterColumn, object masterValue, out TableInfo[] tables)
        {
            tables = null;
            // TODO:  Add XmlDataSource.LoadRefObjectList implementation
            return null;
        }

        public override void MakeTuple(string tableName, string leftColumn, string rightColumn, object leftVal, object rightVal, int mode)
        {
            // TODO:  Add XmlDataSource.MakeTuple implementation
        }

        public override void BeginSaveChanges()
        {
        }

        public override void FinishSaveChanges()
        {
        }

        public override void SaveObjectChanges(Sooda.SoodaObject obj, bool isPrecommit)
        {
            // TODO:  Add XmlDataSource.SaveObjectChanges implementation
        }

        public override IDataReader LoadObjectTable(SoodaObject obj, object keyValue, int tableNumber, out TableInfo[] tables)
        {
            tables = null;
            return null;
        }

        public override IDataReader ExecuteQuery(Sooda.QL.SoqlQueryExpression expr, SchemaInfo schema, object[] parameters)
        {
            return null;
        }

        public override IDataReader ExecuteRawQuery(string expr, object[] parameters)
        {
            return null;
        }

        public override int ExecuteNonQuery(string expr, object[] parameters)
        {
            return 0;
        }

        public override bool IsOpen
        {
            get { return false; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return IsolationLevel.ReadCommitted; }
            set { }
        }
    }
}
